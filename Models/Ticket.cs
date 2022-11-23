using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace BugTracker.Models
{
    public class Ticket
    {
        // Primary Key / PK ?!
        public int Id { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and max {1} characters long.", MinimumLength = 2)]
        [DisplayName("Ticket Title")]
        public string? Title { get; set; }

        [Required]
        [StringLength(2000, ErrorMessage = "The {0} must be at least {2} and max {1} characters long.", MinimumLength = 2)]
        [DisplayName("Ticket Description")]
        public string? Description { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayName("Date Created")]
        public DateTime DateCreated { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayName("Date Updated")]
        public DateTime? LastUpdated { get; set; }

        public bool Archived { get; set; }

        [DisplayName("Archived By Project")]
        public bool ArchivedByProject { get; set; }

        // Foreign Key / FK ?!
        public int ProjectId { get; set; }

        // Foreign Key / FK ?!
        public int TicketTypeId { get; set; }

        // Foreign Key / FK ?!
        public int TicketStatusId { get; set; }

        // Foreign Key / FK ?!
        public int TicketPriorityId { get; set; }

        // Foreign Key / FK ?!
        public string? DeveloperUserId { get; set; }

        // Foreign Key / FK ?!
        [Required]
        public string? SubmitterUserId { get; set; }


        // Navigation Properties

        // Add a relationship to the Category model
        public virtual Project? Project { get; set; }

        [DisplayName("Priority")]
        public virtual TicketPriority? TicketPriority { get; set; }
        [DisplayName("Type")]
        public virtual TicketType? TicketType { get; set; }
        [DisplayName("Status")]
        public virtual TicketStatus? TicketStatus { get; set; }

        [DisplayName("Ticket Developer")]
        public virtual BTUser? DeveloperUser { get; set; }

        [DisplayName("Submitted By")]
        public virtual BTUser? SubmitterUser { get; set; }

        // Add a relationship to the Comment model
        public virtual ICollection<TicketComment> Comments { get; set; } = new HashSet<TicketComment>();

        // Add a relationship to the Tag model
        public virtual ICollection<TicketAttachment> Attachments { get; set; } = new HashSet<TicketAttachment>();

        public virtual ICollection<Notification> Notifications { get; set; } = new HashSet<Notification>();

        public virtual ICollection<TicketHistory> History { get; set; } = new HashSet<TicketHistory>();
    }
}
