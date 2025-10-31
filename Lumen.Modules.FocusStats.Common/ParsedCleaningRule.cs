namespace Lumen.Modules.FocusStats.Common {
    public class ParsedCleaningRule {
        public string Replacement { get; set; } = null!;
        public string Target { get; set; } = null!;
        public Dictionary<string, string> Tests { get; set; } = null!;
    }
}
