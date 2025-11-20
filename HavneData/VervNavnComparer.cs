using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HavneData
{
    public class VervNavnComparer : IEqualityComparer<(string, string)>
    {
        public bool Equals((string, string) x, (string, string) y)
        {
            return x.Item1.Equals(y.Item1);
        }

        public int GetHashCode((string, string) obj)
        {
            return obj.Item1.GetHashCode();
        }
    }
}
