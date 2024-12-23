namespace Lumen.Module.FocusStats.Business {
    public class ParsedRule {
        public string Replacement { get; set; } = null!;
        public string Target { get; set; } = null!;
        public Dictionary<string, string> Tests { get; set; } = null!;
    }
}
