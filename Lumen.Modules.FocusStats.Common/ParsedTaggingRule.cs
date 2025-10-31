namespace Lumen.Modules.FocusStats.Common {
    public class ParsedTaggingRule {
        public IEnumerable<string> Tags { get; set; } = [];
        public string Target { get; set; } = null!;
        public Dictionary<string, bool> Tests { get; set; } = null!;
    }
}
