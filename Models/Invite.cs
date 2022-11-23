using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace BugTracker.Models
{
    public class Invite
    {
        // Primary Key / PK ?!
        public int Id { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayName("Date Sent")]
        public DateTime InviteDate { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayName("Date Joined")]
        public DateTime? JoinDate { get; set; }

        public Guid CompanyToken { get; set; }

        // Foreign Key / FK ?!
        public int CompanyId { get; set; }

        // Foreign Key / FK ?!
        public int ProjectId { get; set; }

        // Foreign Key / FK ?!
        [Required]
        public string? InvitorId { get; set; }

        // Foreign Key / FK ?!
        public string? InviteeId { get; set; }

        [Required]
        [DisplayName("Email")]
        public string? InviteeEmail { get; set; }

        // [StringLength(50, ErrorMessage = "The {0} must be at least {2} and max {1} characters long.", MinimumLength = 2)]
        [Required]
        [Display(Name = "First Name")]
        public string? InviteeFirstName { get; set; }

        // [StringLength(50, ErrorMessage = "The {0} must be at least {2} and max {1} characters long.", MinimumLength = 2)]
        [Required]
        [Display(Name = "Last Name")]
        public string? InviteeLastName { get; set; }

        public string? Message { get; set; }

        public bool IsValid { get; set; }


        // Navigation Properties

        // Add a relationship to the Category model
        public virtual Company? Company { get; set; }

        // Add a relationship to the Comment model
        public virtual Project? Project { get; set; }

        // Add a relationship to the Tag model
        public virtual BTUser? Invitor { get; set; }

        public virtual BTUser? Invitee { get; set; }
    }
}
