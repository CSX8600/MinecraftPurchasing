using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClussPro.Base.Extensions
{
    public static class HashSetExt
    {
        public static void AddRange<T>(this HashSet<T> hashSet, IEnumerable<T> range)
        {
            if (hashSet == null || range == null)
            {
                return;
            }
            foreach(T item in range)
            {
                hashSet.Add(item);
            }
        }
    }
}
