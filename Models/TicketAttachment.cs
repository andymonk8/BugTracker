using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using BugTracker.Extensions;

namespace BugTracker.Models
{
    public class TicketAttachment
    {
        // Primary Key
        public int Id { get; set; }

        [DisplayName("File Description")]
        [StringLength(2000, ErrorMessage = "The {0} must be at least {2} and max {1} characters long.", MinimumLength = 2)]
        public string? Description { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayName("Date Added")]
        public DateTime DateCreated { get; set; }

        // Foreign Key / FK ?!
        public int TicketId { get; set; }

        // Foreign Key / FK ?!
        [Required]
        public string? UserId { get; set; }

        // (" ** Just like our image properties we will capture file data in the exact same way ")
        [NotMapped]
        [DisplayName("Select a file")]
        [DataType(DataType.Upload)]
        [MaxFileSize(1024 * 1024)]
        [AllowedExtensions(new string[] { ".jpg", ".png", ".doc", ".docx", ".xls", ".xlsx", ".pdf" })]
        public IFormFile? FormFile { get; set; }

        [DisplayName("File Name")]
        public string? FileName { get; set; }

        [DisplayName("File Attachment")]
        public byte[]? FileData { get; set; }

        [DisplayName("File Extension")]
        public string? FileType { get; set; }


        // Navigation Properties
        public virtual Ticket? Ticket { get; set; }

        // Add the relationship to the BlogUser
        [DisplayName("Team Member")]
        public virtual BTUser? User { get; set; }
    }
}