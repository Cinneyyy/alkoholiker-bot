using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace src.Ext;

public static class TypeExt
{
    private static readonly Type[] valueTupleTypes =
    [
        typeof(ValueTuple<>),
        typeof(ValueTuple<,>),
        typeof(ValueTuple<,,>),
        typeof(ValueTuple<,,,>),
        typeof(ValueTuple<,,,,>),
        typeof(ValueTuple<,,,,,>),
        typeof(ValueTuple<,,,,,,>),
        typeof(ValueTuple<,,,,,,,>)
    ];
    private static readonly Type[] referenceTupleTypes =
    [
        typeof(Tuple<>),
        typeof(Tuple<,>),
        typeof(Tuple<,,>),
        typeof(Tuple<,,,>),
        typeof(Tuple<,,,,>),
        typeof(Tuple<,,,,,>),
        typeof(Tuple<,,,,,,>),
        typeof(Tuple<,,,,,,,>)
    ];


    extension(Type type)
    {
        public bool IsValueTuple => type.IsGenericType && valueTupleTypes.Contains(type.GetGenericTypeDefinition());
        public bool IsReferenceTuple => type.IsGenericType && referenceTupleTypes.Contains(type.GetGenericTypeDefinition());
        public bool IsTuple => type.IsValueTuple || type.IsReferenceTuple;


        public bool ImplementsInterface(Type interfaceType)
            => type.GetInterfaces().Any(i => (i.IsGenericType && i.GetGenericTypeDefinition() == interfaceType) || i == interfaceType);

        public IEnumerable<FPInfo> GetInheritedFPInfos(BindingFlags bindingFlags)
        {
            while(type is not null && type != typeof(object))
            {
                foreach(FPInfo info in type.GetFPInfos(bindingFlags))
                    yield return info;

                type = type.BaseType!;
            }
        }

        public FPInfo GetInheritedFPInfo(string name, BindingFlags flags)
            => type.GetInheritedFPInfos(flags).FirstOrDefault(i => i.Name == name);

        public FPInfo GetFPInfo(string name, BindingFlags flags)
            => new(fInfo: type.GetField(name, flags), type.GetProperty(name, flags));

        public IEnumerable<FPInfo> GetFPInfos(BindingFlags flags)
            => FPInfo.Join(type.GetFields(flags), type.GetProperties(flags));
    }


    extension<T>(T) where T : struct, Enum
    {
        public static i32 DefinedValueCount => typeof(T).GetEnumValues().Length;
    }
}
