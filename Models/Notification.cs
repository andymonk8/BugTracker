using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace BugTracker.Models
{
    public class Notification
    {
        // Primary Key / PK ?!
        public int Id { get; set; }

        // Foreign Key / FK ?!
        public int ProjectId { get; set; }

        // Foreign Key / FK ?!
        public int TicketId { get; set; }

        // [StringLength(100, ErrorMessage = "The {0} must be at least {2} and max {1} characters long.", MinimumLength = 2)]
        [Required]
        public string? Title { get; set; }

        [Required]
        public string? Message { get; set; }

        // [DataType(DataType.Date)]
        [DataType(DataType.DateTime)]
        [DisplayName("Date Created")]
        public DateTime DateCreated { get; set; }

        // Foreign Key / FK ?!
        [Required]
        public string? SenderId { get; set; }

        // Foreign Key / FK ?!
        [Required]
        public string? RecipientId { get; set; }

        // Foreign Key ?! / FK ?!
        public int NotificationTypeId { get; set; }

        [DisplayName("Has Been Read")]
        public bool HasBeenViewed { get; set; }


        // Navigation Properties

        // Add a relationship to the Category model
        public virtual NotificationType? NotificationType { get; set; }

        // Add a relationship to the Comment model
        public virtual Ticket? Ticket { get; set; }

        // Add a relationship to the Tag model
        public virtual Project? Project { get; set; }

        public virtual BTUser? Sender { get; set; }

        public virtual BTUser? Recipient { get; set; }
    }
}
