using System.Text.RegularExpressions;

namespace Lumen.Modules.FocusStats.Common.Rules {
    public class TaggingRule {
        public Regex Regex { get; }
        public IEnumerable<string> Tags { get; }
        public RuleTarget Target { get; }
        public Dictionary<string, bool> Tests { get; }

        public TaggingRule(string regexp, IEnumerable<string> tags, RuleTarget target, Dictionary<string, bool> tests) {
            Regex = new Regex(regexp, RegexOptions.Compiled);
            Tags = tags;
            Target = target;
            Tests = tests;

            foreach (var test in Tests) {
                if (Regex.IsMatch(test.Key) != test.Value) {
                    throw new InvalidDataException($"Invalid Rule! Test {test.Key} => {test.Value} not validated at runtime!");
                }
            }
        }
    }
}
