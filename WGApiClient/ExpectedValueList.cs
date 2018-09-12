﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WGApi
{
    public interface IExpectedValueList
    {
        Task Initialize();
        Dictionary<int, ExpectedValues> this[string version] { get; }
        IEnumerable<string> Versions { get; }
    }

    public class VbaddictExpectedValueList : ExpectedValueListBase
    {
        protected override string ExclAbsMinVersion => "-1";
        protected override string CheckAtLeastUntilVersion => "29";
        protected override string LinkFormat => "http://www.wnefficiency.net/exp/expected_tank_values_{0}.json";
        protected override string FileName => "expectedValues.json";
        protected override IEnumerable<string> CreateVersionValues(string start)
        {
            int version = int.Parse(start);
            while (true)
            {
                version += 1;
                yield return version.ToString();
            }
        }
    }

    public class XvmExpectedValueList : ExpectedValueListBase
    {
        protected override string ExclAbsMinVersion => "2017-10-07";
        protected override string CheckAtLeastUntilVersion => DateTime.Today.ToString("yyyy-MM-dd");
        protected override string LinkFormat => "https://static.modxvm.com/wn8-data-exp/json/wn8exp-{0}.json";
        protected override string FileName => "xvmExpectedValues.json";
        protected override IEnumerable<string> CreateVersionValues(string start)
        {
            DateTime date = DateTime.Parse(start);
            while (true)
            {
                date = date.AddDays(1);
                yield return date.ToString("yyyy-MM-dd");
            }
        }

        private SecurityProtocolType OriginalSecurityProtcol;

        protected override void PrepareWebSettings()
        {
            base.PrepareWebSettings();

            OriginalSecurityProtcol = ServicePointManager.SecurityProtocol;
            // https://social.msdn.microsoft.com/Forums/en-US/920862c4-8136-4817-adf8-2a8ccdeb0073/httpwebrequest-rest-call-authentication-failed-because-the-remote-party-has-closed-the-transport?forum=netfxbcl
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11;
        }

        protected override void CleanupWebSettings()
        {
            ServicePointManager.SecurityProtocol = OriginalSecurityProtcol;

            base.CleanupWebSettings();
        }
    }

    public abstract class ExpectedValueListBase : IExpectedValueList
    {
        public event EventHandler ValuesLoaded;

        private void OnValuesLoaded()
        {
            ValuesLoaded?.Invoke(this, new EventArgs());
        }

        protected abstract string LinkFormat { get; }

        private string _Directory;
        private string Directory
        {
            get
            {
                if (_Directory == null)
                    _Directory = Path.GetFullPath("expected_values");
                return _Directory;
            }
        }

        protected abstract string FileName { get; }
        private string SaveFilePath { get { return Path.Combine(Directory, FileName); } }

        protected Dictionary<string, Dictionary<int, ExpectedValues>> Values;

        public Dictionary<int, ExpectedValues> this[string version] { get { return Values[version]; } }
        public IEnumerable<string> Versions { get { return Values.Select(p => p.Key); } }

        protected abstract string ExclAbsMinVersion { get; }
        protected abstract string CheckAtLeastUntilVersion { get; }

        public ExpectedValueListBase()
        {
            System.IO.Directory.CreateDirectory(Directory);
        }

        protected virtual void PrepareWebSettings() { }
        protected virtual void CleanupWebSettings() { }

        public async Task Initialize()
        {
            PrepareWebSettings();

            if (Open(SaveFilePath))
                await AddNewValues(Versions.Last());
            else
            {
                Values = new Dictionary<string, Dictionary<int, ExpectedValues>>();
                await AddNewValues(ExclAbsMinVersion, CheckAtLeastUntilVersion);
            }
            Values = Values.OrderBy(kvp => kvp.Key).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            Save(SaveFilePath);

            CleanupWebSettings();
        }

        /// <param name="start">exclusive</param>
        protected abstract IEnumerable<string> CreateVersionValues(string start);

        private bool Open(string path)
        {
            try
            {
                using (Stream stream = File.OpenRead(path))
                using (StreamReader reader = new StreamReader(stream))
                    Values = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<int, ExpectedValues>>>(reader.ReadToEnd());
                return true;
            }
            catch (FileNotFoundException)
            {
                return false;
            }
        }

        private void Save(string path)
        {
            using (Stream stream = File.Create(path))
            using (StreamWriter writer = new StreamWriter(stream))
                writer.Write(JsonConvert.SerializeObject(Values));
        }

        /// <param name="latestVersion">exclusive</param>
        private async Task AddNewValues(string latestVersion, string checkAtLeastTo = null)
        {
            List<Task<bool>> tasks = new List<Task<bool>>();
            bool waitForTasks = true;
            bool reachedMinVersion = false;
            foreach (string version in CreateVersionValues(latestVersion))
            {
                //get all values until checkAtLeastTo at once, might be unordered, but is definitely faster
                if (!reachedMinVersion)
                    tasks.Add(TryAddNewValues(version));
                else if (waitForTasks)
                {
                    await Task.WhenAll(tasks);
                    waitForTasks = false;
                }
                else if (!await TryAddNewValues(version))
                    break;

                reachedMinVersion |= version == (checkAtLeastTo ?? version);
            }
        }

        private async Task<bool> TryAddNewValues(string version)
        {
            ApiWrapper<ExpectedValues[]> apiObject;
            try
            {
                apiObject = JsonConvert.DeserializeObject<ApiWrapper<ExpectedValues[]>>(await GetApiResponse(version));
            }
            catch
            {
                return false;
            }
            if (apiObject.Header.Version != version)
                throw new Exception("expected-value-version is not the requested version");

            try
            {
                Values.Add(version, apiObject.Data.ToDictionary(v => v.TankID));
            }
            catch (ArgumentException) { }
            return true;
        }

        private async Task<string> GetApiResponse(string version)
        {
            WebRequest request = WebRequest.Create(String.Format(LinkFormat, version));
            using (var response = await request.GetResponseAsync())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
                return reader.ReadToEnd();
        }


        private class ApiWrapper<T>
        {
            [JsonProperty("header")]
            public Header Header { get; set; }
            [JsonProperty("data")]
            public T Data { get; set; }
        }

        private class Header
        {
            [JsonProperty("version")]
            public string Version { get; set; }
        }
    }
}
