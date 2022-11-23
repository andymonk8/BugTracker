using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace BugTracker.Models
{
    public class Company
    {
        // Primary Key / PK ?!
        public int Id { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} and max {1} characters long.", MinimumLength = 2)]
        [DisplayName("Company Name")]
        public string? Name { get; set; }

        [StringLength(2000, ErrorMessage = "The {0} must be at least {2} and max {1} characters long.", MinimumLength = 2)]
        [DisplayName("Company Description")]
        public string? Description { get; set; }

        [NotMapped]
        public IFormFile? CompanyImage { get; set; }

        public byte[]? ImageData { get; set; }
        public string? ImageType { get; set; }

        
        // Navigation Properties
        public virtual ICollection<Project> Projects { get; set; } = new HashSet<Project>();
        public virtual ICollection<BTUser> Members { get; set; } = new HashSet<BTUser>();
        public virtual ICollection<Invite> Invites { get; set; } = new HashSet<Invite>();
    }
}
