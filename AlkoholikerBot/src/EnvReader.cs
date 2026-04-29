using System;
using System.IO;

namespace src;

public static class EnvReader
{
    public static void Load(string filePath)
    {
        if(!File.Exists(filePath))
            return;

        foreach(string line in File.ReadAllLines(filePath))
        {
            string trimmed = line.Trim();

            if(string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith('#'))
                continue;

            i32 separatorIndex = trimmed.IndexOf('=');
            if(separatorIndex <= 0)
                continue;

            string key = trimmed[..separatorIndex].Trim();
            string value = trimmed[(separatorIndex + 1)..].Trim();

            Environment.SetEnvironmentVariable(key, value);
        }
    }
}