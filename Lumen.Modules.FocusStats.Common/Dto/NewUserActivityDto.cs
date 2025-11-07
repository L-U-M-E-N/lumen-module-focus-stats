using System.ComponentModel.DataAnnotations;

namespace Lumen.Modules.FocusStats.Common.Dto {
    public class NewUserActivityDto {
        [Required]
        public DateTime StartTime { get; set; }
        [Required]
        public int SecondsDuration { get; set; }
        [Required]
        public string AppOrExe { get; set; } = null!;
        [Required]
        public string Name { get; set; } = null!;
        [Required]
        public string Device { get; set; } = null!;
    }
}
