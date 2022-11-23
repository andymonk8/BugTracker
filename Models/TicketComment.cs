using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace BugTracker.Models
{
    public class TicketComment
    {
        // Primary Key / PK ?!
        public int Id { get; set; }

        [Required]
        [DisplayName("Member Comment")]
        [StringLength(5000, ErrorMessage = "The {0} must be at least {2} and max {1} characters long.", MinimumLength = 2)]
        public string? Comment { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayName("Created Date")]
        public DateTime DateCreated { get; set; }

        // Foreign Key / FK ?!
        public int TicketId { get; set; }

        // Foreign Key / FK ?!
        [Required]
        public string? UserId { get; set; }


        // Navigation Properties
        [DisplayName("Ticket")]
        public virtual Ticket? Ticket { get; set; }

        // Add the relationship to the BlogUser
        [DisplayName("Team Member")]
        public virtual BTUser? User { get; set; }
    }
}
