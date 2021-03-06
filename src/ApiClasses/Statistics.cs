﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WGApi
{
    public class Statistics
    {
        [JsonProperty("spotted")]
        public int Spotted { get; set; }
        [JsonProperty("battles")]
        public int Battles { get; set; }
        [JsonProperty("wins")]
        public int Victories { get; set; }
        [JsonProperty("damage_dealt")]
        public int Damage { get; set; }
        [JsonProperty("xp")]
        public int Experience { get; set; }
        [JsonProperty("frags")]
        public int Frags { get; set; }
        [JsonProperty("survived_battles")]
        public int SurvivedBattles { get; set; }
        [JsonProperty("capture_points")]
        public int Cap { get; set; }
        [JsonProperty("dropped_capture_points")]
        public int Decap { get; set; }
        [JsonProperty("shots")]
        public int Shots { get; set; }
        [JsonProperty("hits")]
        public int Hits { get; set; }
        [JsonProperty("draws")]
        public int Draws { get; set; }
        [JsonProperty("damage_received")]
        public int DamageReceived { get; set; }

        

        public Statistics() { }

        public Statistics(Statistics statistics)
        {
            Spotted = statistics.Spotted;
            Battles = statistics.Battles;
            Victories = statistics.Victories;
            Damage = statistics.Damage;
            Experience = statistics.Experience;
            Frags = statistics.Frags;
            SurvivedBattles = statistics.SurvivedBattles;
            Cap = statistics.Cap;
            Decap = statistics.Decap;
            Shots = statistics.Shots;
            Hits = statistics.Hits;
            Draws = statistics.Draws;
            DamageReceived = statistics.DamageReceived;
        }

        public static Statistics operator +(Statistics first, Statistics second)
        {
            return Operate(first, second, (f, s) => f + s);
        }

        public static Statistics operator -(Statistics first, Statistics second)
        {
            return Operate(first, second, (f, s) => f - s);
        }

        private static Statistics Operate(Statistics first, Statistics second, Func<int, int, int> operation) => Operate(first, second, 0, operation);
        private static Statistics Operate(Statistics stats, int i, Func<int, int, int> operation) => Operate(stats, null, i, operation);
        private static Statistics Operate(Statistics first, Statistics second, int i, Func<int, int, int> operation)
        {
            return new Statistics
            {
                Spotted = operation(first.Spotted, second?.Spotted ?? i),
                Battles = operation(first.Battles, second?.Battles ?? i),
                Victories = operation(first.Victories, second?.Victories ?? i),
                Damage = operation(first.Damage, second?.Damage ?? i),
                Experience = operation(first.Experience, second?.Experience ?? i),
                Frags = operation(first.Frags, second?.Frags ?? i),
                SurvivedBattles = operation(first.SurvivedBattles, second?.SurvivedBattles ?? i),
                Cap = operation(first.Cap, second?.Cap ?? i),
                Decap = operation(first.Decap, second?.Decap ?? i),
                Shots = operation(first.Shots, second?.Shots ?? i),
                Hits = operation(first.Hits, second?.Hits ?? i),
                Draws = operation(first.Draws, second?.Draws ?? i),
                DamageReceived = operation(first.DamageReceived, second?.DamageReceived ?? i),
            };
        }
    }
}
