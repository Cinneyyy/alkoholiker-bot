using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace src;

public sealed class FPInfo(FieldInfo fInfo = null, PropertyInfo pInfo = null)
{
    public FieldInfo FInfo { get; } = fInfo;
    public PropertyInfo PInfo { get; } = pInfo;
    public bool IsField { get; } = fInfo is null == pInfo is null
        ? throw new($"Just/only one of `fInfo` and `pInfo` must be null in `FPInfo(fInfo, pInfo)`.")
        : fInfo is not null;
    public string Name => IsField ? FInfo.Name : PInfo.Name;
    public Type Type => IsField ? FInfo.FieldType : PInfo.PropertyType;
    public Type DeclaringType => IsField ? FInfo.DeclaringType : PInfo.DeclaringType;
    public bool CanRead => IsField || PInfo.CanRead;
    public bool CanWrite => IsField ? !FInfo.IsLiteral && !FInfo.IsInitOnly : PInfo.CanWrite;


    public object GetValue(object obj)
        => IsField ? FInfo.GetValue(obj) : PInfo.GetValue(obj);

    public void SetValue(object obj, object value)
    {
        if(IsField) FInfo.SetValue(obj, value);
        else PInfo.SetValue(obj, value);
    }

    public T GetCustomAttribute<T>() where T : Attribute
        => IsField ? FInfo.GetCustomAttribute<T>() : PInfo.GetCustomAttribute<T>();


    public static IEnumerable<FPInfo> Join(IEnumerable<FieldInfo> fieldInfos, IEnumerable<PropertyInfo> propertyInfos)
        => fieldInfos
        .Select(i => new FPInfo(fInfo: i))
        .Concat(
            propertyInfos
            .Select(i => new FPInfo(pInfo: i))
        );
}
