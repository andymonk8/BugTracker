using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace BugTracker.Models
{
    public class Project
    {
        // Primary Key / PK ?!
        public int Id { get; set; }

        // Foreign Key /  FK ?!
        public int CompanyId { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} and max {1} characters long.", MinimumLength = 2)]
        [DisplayName("Project Name")]
        public string? Name { get; set; }

        [Required]
        [StringLength(2000, ErrorMessage = "The {0} must be at least {2} and max {1} characters long.", MinimumLength = 2)]
        [DisplayName("Project Description")]
        public string? Description { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayName("Date Created")]
        public DateTime DateCreated { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayName("Project Start Date")]
        public DateTime? StartDate { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayName("Project End Date")]
        public DateTime? EndDate { get; set; }

        // Foreign Key ?! / FK ?!
        public int ProjectPriorityId { get; set; }

        [NotMapped]
        [DataType(DataType.Upload)]
        public IFormFile? ProjectImage { get; set; }

        [DisplayName("Project Image")]
        public byte[]? ImageData { get; set; }

        [DisplayName("File Extension")]
        public string? ImageType { get; set; }

        public bool Archived { get; set; }


        // Navigation Properties

        // Add a relationship to the Category model
        public virtual Company? Company { get; set; }

        // Add a relationship to the Comment model
        public virtual ProjectPriority? ProjectPriority { get; set; }

        // Add a relationship to the Tag model
        public virtual ICollection<BTUser> Members { get; set; } = new HashSet<BTUser>();

        public virtual ICollection<Ticket> Tickets { get; set; } = new HashSet<Ticket>();
    }
}
