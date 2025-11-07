using System.Text.RegularExpressions;

namespace Lumen.Modules.FocusStats.Common.Rules {
    public class CleaningRule {
        public Guid Id { get; set; }
        public Regex Regex { get; } = null!;
        public string Replacement { get; } = null!;
        public RuleTarget Target { get; }
        public Dictionary<string, string> Tests { get; } = null!;

        protected CleaningRule() { }
        public CleaningRule(string regexp, string replacement, RuleTarget target, Dictionary<string, string> tests) {
            Regex = new Regex(regexp, RegexOptions.Compiled);
            Replacement = replacement;
            Target = target;
            Tests = tests;

            foreach (var test in Tests) {
                if (test.Value != Clean(test.Key)) {
                    throw new InvalidDataException($"Invalid Rule! Test {test.Key} => {test.Value} not validated at runtime!");
                }
            }
        }

        public string Clean(string input) {
            var ret = Regex.Replace(input, Replacement);

            return ret;
        }
    }
}
