using Lumen.Modules.FocusStats.Common.Rules;

namespace Lumen.Modules.FocusStats.Common {
    public class UserFocusedActivity {
        public DateTime StartTime { get; set; }
        public int SecondsDuration { get; set; }
        public string ProgramExe { get; set; }
        public string ProgramName { get; set; }

        public static UserFocusedActivity CreateActivityAsync(IEnumerable<CleaningRule> replacementRules, DateTime start, int duration, string exe, string name) {
            name = CleanString(replacementRules, name, RuleTarget.Name);
            exe = CleanString(replacementRules, exe, RuleTarget.Exe);

            if (string.IsNullOrWhiteSpace(exe)) {
                throw new ArgumentNullException(nameof(exe));
            }

            if (string.IsNullOrWhiteSpace(name)) {
                name = "?";
            }

            return new(start, duration, exe, name);
        }

        public UserFocusedActivity() { }

        protected UserFocusedActivity(DateTime start, int duration, string exe, string name) {
            StartTime = start;
            SecondsDuration = duration;
            ProgramExe = exe;
            ProgramName = name;
        }

        private static string CleanString(IEnumerable<CleaningRule> rules, string input, RuleTarget targetType) {
            string output = input.Trim();

            foreach (var rule in rules) {
                if (rule.GetTarget() == targetType || rule.GetTarget() == RuleTarget.Both) {
                    output = rule.Clean(output);
                }
            }

            return output;
        }
    }
}
