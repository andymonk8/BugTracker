using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace BugTracker.Models
{
    public class TicketHistory
    {
        // Primary Key / PK ?!
        public int Id { get; set; }

        // Foreign Key / FK ?!
        public int TicketId { get; set; }

        // (" [StringLength(50, ErrorMessage = "The {0} must be at least {2} and max {1} characters long.", MinimumLength = 2)] ")
        [DisplayName("Updated Ticket Property")]
        public string? PropertyName { get; set; }

        // (" [StringLength(2000, ErrorMessage = "The {0} must be at least {2} and max {1} characters long.", MinimumLength = 2)] ")
        [StringLength(5000, ErrorMessage = "The {0} must be at least {2} and max {1} characters long.", MinimumLength = 2)]
        [DisplayName("Description of Change")]
        public string? Description { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayName("Date Created")]
        public DateTime DateCreated { get; set; }

        [DisplayName("Previous Value")]
        public string? OldValue { get; set; }

        [DisplayName("Current Value")]
        public string? NewValue { get; set; }

        // Foreign Key?! / FK ?!
        [Required]
        public string? UserId { get; set; }


        // Navigation Properties

        // Add a relationship to the Category model
        public virtual Ticket? Ticket { get; set; }

        public virtual BTUser? User { get; set; }
    }
}