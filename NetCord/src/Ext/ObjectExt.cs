using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace src.Ext;

public static class ObjectExt
{
    extension(object obj)
    {
        public string ToStringCatchCollection()
        {
            if(obj is null)
                return "null";

            Type type = obj.GetType();

            if(type == typeof(string))
                return obj as string;

            if(type.ImplementsInterface(typeof(ITuple)))
            {
                bool isValueTuple = type.IsValueTuple;
                bool isRefTuple = type.IsReferenceTuple;

                if(!isValueTuple && !isRefTuple)
                    return obj.ToString() ?? "null";

                List<object> values = obj.GetTupleValues(isValueTuple);

                StringBuilder sb = new("(");
                sb.Append(values.ToStringFromCollection()[1..^1]);
                sb.Append(')');

                return sb.ToString();
            }

            if(type.ImplementsInterface(typeof(IEnumerable<>)))
            {
                MethodInfo toString = typeof(IEnumerableExt).GetMethod(nameof(IEnumerableExt.ToStringFromCollection));
                Type ienumerableType = obj
                    .GetType()
                    .GetInterfaces()
                    .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));

                Type ienumerableT = ienumerableType.GetGenericArguments().First();

                return toString?.MakeGenericMethod(ienumerableT)?.Invoke(null, [obj]) as string ?? "null";
            }

            return obj.ToString() ?? "null";
        }

    }

    extension(object obj)
    {
        public List<object> GetTupleValues(bool isValueTuple)
        {
            List<object> ret = [];
            Type type = obj.GetType();

            if(isValueTuple)
            {
                foreach(FieldInfo f in type.GetFields())
                    if(f.Name == "Rest")
                    {
                        if(f.GetValue(obj) is object nested)
                            ret.AddRange(GetTupleValues(nested, true));
                    }
                    else
                        ret.Add(f.GetValue(obj));
            }
            else
            {
                foreach(PropertyInfo p in type.GetProperties())
                    if(p.Name == "Rest")
                    {
                        if(p.GetValue(obj) is object nested)
                            ret.AddRange(GetTupleValues(nested, true));
                    }
                    else
                        ret.Add(p.GetValue(obj));
            }

            return ret;
        }
    }
}
