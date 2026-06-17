using NetCord.Gateway;

namespace src.Rules;

public static class RuleMgr
{
    private static readonly List<Rule> rules = [];


    public static string path { get; private set; }


    public static IEnumerable<string> GetRuleFiles()
        => Directory.GetFiles(path, "*.json", SearchOption.AllDirectories).Concat(Directory.GetFiles(path, "*.jsonc", SearchOption.AllDirectories));


    /// <summary>Unloads and reloads all rules.</summary>
    public static IEnumerable<string> Load()
    {
        rules.Clear();

        IEnumerable<string> files = GetRuleFiles();
        foreach(string file in files)
        {
            try
            {
                rules.AddRange(Json.DeserializeFile<Rule[]>(file));
            }
            catch(Exception e)
            {
                Log.Out($"Failed to load rule file ({file}): {e.Message}.");
            }
        }

        rules.Sort((a, b) => a.order - b.order);
        return files;
    }

    public static void SetPath(string path)
        => RuleMgr.path = path;

    /// <summary>Returns: number of rules applied.</summary>
    public static async Task<u32> ApplyRules(Message message)
    {
        u32 numRulesApplied = 0;

        foreach(Rule rule in rules)
        {
            try
            {
                if(rule.predicate.CheckTruth(rule, message))
                {
                    if(rule.useRandomReply)
                        await rule.randomReply.Apply(message, rule);
                    else
                    {
                        foreach(Reply reply in rule.replies)
                            await reply.Apply(message, rule);
                    }

                    numRulesApplied++;

                    if(rule.@break)
                        break;
                }
            }
            catch(Exception e)
            {
                Log.Out($"Exception occurred when checking/applying rule: {e}");
            }
        }

        return numRulesApplied;
    }
}
