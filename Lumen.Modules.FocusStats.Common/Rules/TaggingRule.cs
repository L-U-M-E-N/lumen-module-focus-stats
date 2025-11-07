using System.Text.RegularExpressions;

namespace Lumen.Modules.FocusStats.Common.Rules {
    public class TaggingRule {
        public Guid Id { get; set; }
        public Regex Regex { get; } = null!;
        public ICollection<string> Tags { get; } = null!;
        public RuleTarget Target { get; }
        public Dictionary<string, bool> Tests { get; } = null!;

        protected TaggingRule() { }
        public TaggingRule(string regexp, ICollection<string> tags, RuleTarget target, Dictionary<string, bool> tests) {
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
