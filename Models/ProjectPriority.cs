using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BugTracker.Models
{
    public class ProjectPriority
    {
        // Primary Key / PK ?!
        public int Id { get; set; }

        // [StringLength(50, ErrorMessage = "The {0} must be at least {2} and max {1} characters long.", MinimumLength = 2)]
        [Required]
        [DisplayName("Project Priority Name")]
        public string? Name { get; set; }
    }
}
