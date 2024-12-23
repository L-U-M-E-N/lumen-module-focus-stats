using System.Text.RegularExpressions;

namespace Lumen.Module.FocusStats.Business.Rules {
    public class CleaningRule {
        public Regex Regex { get; }
        public string Replacement { get; }
        public RuleTarget Target { get; }

        public CleaningRule(string regexp, string replacement, RuleTarget target, Dictionary<string, string> Tests) {
            Regex = new Regex(regexp, RegexOptions.Compiled);
            Replacement = replacement;
            Target = target;

            foreach (var test in Tests) {
                if (test.Value != this.Clean(test.Key)) {
                    throw new InvalidDataException($"Invalid Rule! Test {test.Key} => {test.Value} not validated at runtime!");
                }
            }
        }

        public string Clean(string input) {
            var ret = Regex.Replace(input, Replacement);

            return ret;
        }

        public RuleTarget GetTarget() {
            return Target;
        }
    }
}
