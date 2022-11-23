using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;
using Microsoft.AspNetCore.Identity;

namespace BugTracker.Models
{
    public class BTUser : IdentityUser
    {
        [Required]
        [Display(Name = "First Name")]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} and max {1} characters long.", MinimumLength = 2)]
        public string? FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} and max {1} characters long.", MinimumLength = 2)]
        public string? LastName { get; set; }

        [NotMapped]
        [Display(Name = "Full Name")]
        public string? FullName { get { return $"{FirstName} {LastName}"; } }

        // (" public IFormFile? AvatarFormFile { get; set; } ")
        [NotMapped]
        [DataType(DataType.Upload)]
        public IFormFile? BTUserImage { get; set; }

        // (" public byte[]? AvatarData { get; set; } ")
        public byte[]? ImageData { get; set; }

        // (" [DisplayName("File Extension")] ")
        // (" public string? AvatarType { get; set; } ")
        public string? ImageType { get; set; }

        // FK / Foreign Key
        public int CompanyId { get; set; }

        // Navigation Properties
        public virtual Company? Company { get; set; }
        public virtual ICollection<Project> Projects { get; set; } = new HashSet<Project>();
    }
}
