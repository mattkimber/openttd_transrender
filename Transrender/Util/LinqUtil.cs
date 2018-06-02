using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Transrender.Util
{
    public static class LinqUtil
    {
        public static T Mode<T>(this IEnumerable<T> input)
        {
            return input.GroupBy(v => v)
                        .OrderByDescending(g => g.Count())
                        .First()
                        .Key;
        }
    }
}
