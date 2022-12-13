using Microsoft.AspNetCore.Mvc.Rendering;

namespace BugTracker.Models.ViewModels
{
    public class AssignDeveloperViewModel
    {
        public Ticket? Ticket { get; set; }
        public MultiSelectList? DeveloperList { get; set; }
        public string? DeveloperId { get; set; }
    }
}
