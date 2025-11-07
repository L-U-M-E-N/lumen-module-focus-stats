using Lumen.Modules.FocusStats.Common.Rules;

namespace Lumen.Modules.FocusStats.Common {
    public class UserFocusedActivity {
        public DateTime StartTime { get; set; }
        public int SecondsDuration { get; set; }
        public string AppOrExe { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Device { get; set; } = null!;
        public List<string> Tags { get; set; } = [];

        public static UserFocusedActivity CreateActivityAsync(IEnumerable<CleaningRule> replacementRules, DateTime start, int duration, string exe, string name, string device) {
            name = CleanString(replacementRules, name, RuleTarget.Name);
            exe = CleanString(replacementRules, exe, RuleTarget.Exe);

            if (string.IsNullOrWhiteSpace(exe)) {
                throw new ArgumentNullException(nameof(exe));
            }

            if (string.IsNullOrWhiteSpace(name)) {
                name = "?";
            }

            return new(start, duration, exe, name, device);
        }

        public UserFocusedActivity() { }

        protected UserFocusedActivity(DateTime start, int duration, string exe, string name, string device) {
            StartTime = start;
            SecondsDuration = duration;
            AppOrExe = exe;
            Name = name;
            Device = device;

        }

        private static string CleanString(IEnumerable<CleaningRule> rules, string input, RuleTarget targetType) {
            string output = input.Trim();

            foreach (var rule in rules) {
                if (rule.Target == targetType || rule.Target == RuleTarget.Both) {
                    output = rule.Clean(output);
                }
            }

            return output;
        }

        public void ApplyNewCleaningRule(CleaningRule rule) {
            if ((rule.Target == RuleTarget.Exe || rule.Target == RuleTarget.Both) && rule.Regex.IsMatch(AppOrExe)) {
                AppOrExe = rule.Clean(AppOrExe);
            }
            if ((rule.Target == RuleTarget.Name || rule.Target == RuleTarget.Both) && rule.Regex.IsMatch(Name)) {
                Name = rule.Clean(Name);
            }
        }

        public void ApplyTaggingRules(IEnumerable<TaggingRule> rules) {
            Tags = [];

            foreach (var rule in rules) {
                if ((rule.Target == RuleTarget.Exe || rule.Target == RuleTarget.Both) && rule.Regex.IsMatch(AppOrExe)) {
                    Tags.AddRange(rule.Tags);
                }
                if ((rule.Target == RuleTarget.Name || rule.Target == RuleTarget.Both) && rule.Regex.IsMatch(Name)) {
                    Tags.AddRange(rule.Tags);
                }
            }

            Tags = [.. Tags.Distinct()];
        }
    }
}
