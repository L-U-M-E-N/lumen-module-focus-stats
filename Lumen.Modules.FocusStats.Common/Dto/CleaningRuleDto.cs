using Lumen.Modules.FocusStats.Common.Rules;

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Lumen.Modules.FocusStats.Common.Dto {
    public class CleaningRuleDto {
        public Guid? Id { get; set; } = null;
        [Required]
        public string Regex { get; set; } = null!;
        [Required]
        public string Replacement { get; set; } = null!;
        [JsonConverter(typeof(JsonStringEnumConverter))]
        [Required]
        public RuleTarget Target { get; set; }
        [Required]
        public Dictionary<string, string> Tests { get; set; } = null!;
    }
}
