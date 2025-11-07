using Lumen.Modules.FocusStats.Common;
using Lumen.Modules.FocusStats.Common.Rules;

using System.Text.Json;

namespace Lumen.Modules.FocusStats.ConsoleApp {
    internal class Program {
        private static readonly List<CleaningRule> CleaningRules = [];
        private static readonly List<TaggingRule> TaggingRules = [];
        private static readonly JsonSerializerOptions jsonSerializerOptions = new() {
            PropertyNameCaseInsensitive = true,
        };

        static void Main(string[] args) {
            Console.WriteLine($"[{DateTime.Now}] Loading settings ...");

            var cleaningRulesDict = JsonSerializer.Deserialize<Dictionary<string, ParsedCleaningRule>>(File.ReadAllText("renaming_rules.json"), jsonSerializerOptions);
            foreach (var item in cleaningRulesDict) {
                CleaningRules.Add(new CleaningRule(item.Key, item.Value.Replacement, Enum.Parse<RuleTarget>(item.Value.Target), item.Value.Tests));
            }

            var taggingRulesDict = JsonSerializer.Deserialize<Dictionary<string, ParsedTaggingRule>>(File.ReadAllText("tagging_rules.json"), jsonSerializerOptions)!;
            foreach (var item in taggingRulesDict) {
                TaggingRules.Add(new TaggingRule(item.Key, item.Value.Tags.ToList(), Enum.Parse<RuleTarget>(item.Value.Target), item.Value.Tests));
            }

            Console.WriteLine($"[{DateTime.Now}] Loading data - Reading file ...");

            var text = File.ReadAllText("history.json");

            Console.WriteLine($"[{DateTime.Now}] Loading data - Read file ! Parsing text ...");

            var data = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, IEnumerable<OldDataFormat>>>>(text, jsonSerializerOptions);

            if (data is null) {
                Console.WriteLine($"[{DateTime.Now}] Parsing failed !");
                return;
            }

            Console.WriteLine($"[{DateTime.Now}] Loading data - Parsed Data ! Convert, clean and compress");
            var dataAsList = new List<UserFocusedActivity>();
            int counter = 0;
            foreach (var itemExe in data) {
                foreach (var itemName in data[itemExe.Key]) {
                    foreach (var item in data[itemExe.Key][itemName.Key]) {
                        try {
                            dataAsList.Add(UserFocusedActivity.CreateActivityAsync(CleaningRules, item.Date, item.Duration, itemExe.Key, itemName.Key, "Unknown"));
                        } catch {
                            Console.WriteLine($"[{itemExe.Key}] [{itemName.Key}] Invalid activity!");
                        }

                        counter++;
                    }
                }
            }

            var distinctExe = dataAsList.Select(x => x.AppOrExe).Distinct().Order();

            Console.WriteLine($"[{DateTime.Now}] Before - Total: {counter} lines.");
            Console.WriteLine($"[{DateTime.Now}] Before - Distinct Exe: {data.Count} lines.");
            Console.WriteLine($"[{DateTime.Now}] Before - Distinct Names: {data.SelectMany(x => x.Value.Keys).Distinct().Count()} lines.");
            Console.WriteLine($"[{DateTime.Now}] After - Total: {dataAsList.Count} lines.");
            Console.WriteLine($"[{DateTime.Now}] After - Distinct Exe: {distinctExe.Count()} lines.");
            var distinctNames = dataAsList.DistinctBy(x => x.Name).Select(x => x.Name).ToList();
            Console.WriteLine($"[{DateTime.Now}] After - Distinct Names: {distinctNames.Count} lines.");

            Console.WriteLine($"[{DateTime.Now}] Writing distinct names and exe ...");

            File.WriteAllText("distinct_exe.json", JsonSerializer.Serialize(distinctExe));
            File.WriteAllText("distinct_names.json", JsonSerializer.Serialize(distinctNames));

            Console.WriteLine($"[{DateTime.Now}] Writing distinct names and exe ... Done!");

            Console.WriteLine($"[{DateTime.Now}] Tagging data ...");

            ulong okCounter = 0;
            ulong totalCounter = 0;
            foreach (var itemExe in data) {
                var matchingExe = TaggingRules.Any((t) => t.Regex.IsMatch(itemExe.Key) && (t.Target == RuleTarget.Both || t.Target == RuleTarget.Exe));

                foreach (var itemName in data[itemExe.Key]) {
                    var matchingName = TaggingRules.Any((t) => t.Regex.IsMatch(itemName.Key) && (t.Target == RuleTarget.Both || t.Target == RuleTarget.Name));

                    foreach (var item in data[itemExe.Key][itemName.Key]) {
                        if (matchingExe || matchingName) {
                            okCounter += (ulong)item.Duration;
                        }
                        totalCounter += (ulong)item.Duration;
                    }
                }
            }

            Console.WriteLine($"[{DateTime.Now}] Tagging data ... Done!");
            Console.WriteLine($"[{DateTime.Now}] Tagging data ... Results (time): {okCounter}/{totalCounter} ({100 * okCounter / totalCounter}%)");

            /*dataAsList.CompressData();

            Console.WriteLine($"[{DateTime.Now}] Final - Total: {dataAsList.Count} lines.");
            Console.WriteLine($"[{DateTime.Now}] Final - Distinct Exe: {dataAsList.DistinctBy(x => x.AppOrExe).Count()} lines.");
            Console.WriteLine($"[{DateTime.Now}] Final - Distinct Names: {dataAsList.DistinctBy(x => x.Name).Count()} lines.");*/

            File.WriteAllText("history_cleaned.json", JsonSerializer.Serialize(dataAsList));

            Console.WriteLine($"[{DateTime.Now}] Cleaning complete !");

            Console.ReadLine();
        }
    }
}
