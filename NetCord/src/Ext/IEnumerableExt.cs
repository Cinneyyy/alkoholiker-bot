using System.Collections.Generic;
using System.Linq;

namespace src.Ext;

public static class IEnumerableExt
{
    extension<T>(IEnumerable<T> collection)
    {
        public string ToStringFromCollection()
            => $"[{string.Join(", ", collection.Select(e => e.ToStringCatchCollection()))}]";
    }
}
