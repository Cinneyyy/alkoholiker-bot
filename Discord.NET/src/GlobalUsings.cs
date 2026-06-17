global using i8 = sbyte;
global using u8 = byte;
global using i16 = short;
global using u16 = ushort;
global using i32 = int;
global using u32 = uint;
global using i64 = long;
global using u64 = ulong;
global using f16 = System.Half;
global using f32 = float;
global using f64 = double;
using System.Collections;
using System;
using System.Collections.Generic;

namespace src;

public static class GlobalUsings
{
    public static T SelectRandom<T>(this T[] arr)
        => arr[Random.Shared.Next(arr.Length)];
    public static T SelectRandom<T>(this List<T> list)
        => list[Random.Shared.Next(list.Count)];
}