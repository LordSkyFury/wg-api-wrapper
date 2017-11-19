using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WGApi
{
    [Flags]
    public enum MarksOfExcellence
    {
        None = 0,
        One = 1 << 0,
        Two = 1 << 1,
        Three = 1 << 2,

        All = ~(~0 << 3),
    }
}
