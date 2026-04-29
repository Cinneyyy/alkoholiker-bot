using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace src;

public static class OptOutMgr
{
    private static readonly List<u64> optedOut = [];


    static OptOutMgr()
        => optedOut = [..File.ReadAllLines("optout.txt").Select(u64.Parse)];


    public static bool IsOptedOut(u64 id)
        => optedOut.Contains(id);

    public static bool OptOut(u64 id)
    {
        if(optedOut.Contains(id))
            return false;

        optedOut.Add(id);
        File.WriteAllLines("optout.txt", [..optedOut.Select(id => id.ToString())]);
        return true;
    }

    public static bool OptIn(u64 id)
    {
        if(!optedOut.Remove(id))
            return false;

        File.WriteAllLines("optout.txt", [..optedOut.Select(id => id.ToString())]);
        return true;
    }
}