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
        Zero = 1 << 0,
        One = 1 << 1,
        Two = 1 << 2,
        Three = 1 << 3,

        All = ~(~0 << 4),
    }
}
