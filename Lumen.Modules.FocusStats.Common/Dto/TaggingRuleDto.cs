using Lumen.Modules.FocusStats.Common.Rules;

using System.ComponentModel.DataAnnotations;

namespace Lumen.Modules.FocusStats.Common.Dto {
    public class TaggingRuleDto {
        public Guid? Id { get; set; } = null;
        [Required]
        public string Regex { get; set; } = null!;
        [Required]
        public IEnumerable<string> Tags { get; set; } = null!;
        [Required]
        public RuleTarget Target { get; set; }
        [Required]
        public Dictionary<string, bool> Tests { get; set; } = null!;
    }
}
