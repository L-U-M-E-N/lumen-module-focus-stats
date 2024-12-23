using Lumen.Module.FocusStats.Business;
using Lumen.Module.FocusStats.Business.Helpers;
using Lumen.Module.FocusStats.Business.Rules;

using System.Text.Json;

namespace Lumen.Modules.FocusStats.ConsoleApp {
    internal class Program {
        private static readonly List<CleaningRule> CleaningRules = [];
        private static readonly JsonSerializerOptions jsonSerializerOptions = new() {
            PropertyNameCaseInsensitive = true,
        };

        static void Main(string[] args) {
            Console.WriteLine($"[{DateTime.Now}] Loading settings ...");

            var rulesDict = JsonSerializer.Deserialize<Dictionary<string, ParsedRule>>(File.ReadAllText("rules.json"), jsonSerializerOptions);
            foreach (var item in rulesDict) {
                CleaningRules.Add(new CleaningRule(item.Key, item.Value.Replacement, Enum.Parse<RuleTarget>(item.Value.Target), item.Value.Tests));
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
                            dataAsList.Add(UserFocusedActivity.CreateActivityAsync(CleaningRules, item.Date, item.Duration, itemExe.Key, itemName.Key));
                        } catch {
                            Console.WriteLine($"[{itemExe.Key}] [{itemName.Key}] Invalid activity!");
                        }

                        counter++;
                    }
                }
            }

            Console.WriteLine($"[{DateTime.Now}] Before - Total: {counter} lines.");
            Console.WriteLine($"[{DateTime.Now}] Before - Distinct Exe: {data.Count} lines.");
            Console.WriteLine($"[{DateTime.Now}] Before - Distinct Names: {data.SelectMany(x => x.Value.Keys).Distinct().Count()} lines.");
            Console.WriteLine($"[{DateTime.Now}] After - Total: {dataAsList.Count} lines.");
            Console.WriteLine($"[{DateTime.Now}] After - Distinct Exe: {dataAsList.DistinctBy(x => x.ProgramExe).Count()} lines.");
            var distinctNames = dataAsList.DistinctBy(x => x.ProgramName).Select(x => x.ProgramName).ToList();
            Console.WriteLine($"[{DateTime.Now}] After - Distinct Names: {distinctNames.Count} lines.");

            ActivityCompressionHelper.CompressData(dataAsList);

            Console.WriteLine($"[{DateTime.Now}] Final - Total: {dataAsList.Count} lines.");
            Console.WriteLine($"[{DateTime.Now}] Final - Distinct Exe: {dataAsList.DistinctBy(x => x.ProgramExe).Count()} lines.");
            Console.WriteLine($"[{DateTime.Now}] Final - Distinct Names: {dataAsList.DistinctBy(x => x.ProgramName).Count()} lines.");

            File.WriteAllText("history_cleaned.json", JsonSerializer.Serialize(dataAsList));

            Console.WriteLine($"[{DateTime.Now}] Cleaning complete !");

            Console.ReadLine();
        }
    }
}
