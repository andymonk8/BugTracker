using BugTracker.Models;

namespace BugTracker.Services.Interfaces
{
    public interface IBTTicketService
    {
        public Task<List<Ticket>> GetAllTicketsByCompanyIdAsync(int companyId);
        public Task AddTicketAsync(Ticket ticket);
        public Task<Ticket> GetTicketByIdAsync(int ticketId, int companyId);
        public Task UpdateTicketAsync(Ticket ticket);
        public Task ArchiveTicketAsync(Ticket ticket);
        public Task RestoreTicketAsync(Ticket ticket);
        public Task<IEnumerable<TicketPriority>> GetTicketPrioritiesAync();
        public Task<IEnumerable<TicketStatus>> GetTicketStatusesAsync();
        public Task<IEnumerable<TicketType>> GetTicketTypesAsync();
        public Task<List<Ticket>> GetUnassignedTicketsAsync(int companyId);
        public Task<List<Ticket>> GetTicketByUserIdAsync(string userId, int companyId);
		public Task AddTicketAttachmentAsync(TicketAttachment ticketAttachment);
		public Task<TicketAttachment> GetTicketAttachmentByIdAsync(int ticketAttachmentId);
        public Task<BTUser> GetTicketDeveloperAsync(int ticketId, int companyId);
        public Task<Ticket> GetTicketAsNoTrackingAsync(int ticketId, int companyId);
        public Task AssignDeveloperAsync(int ticketId, string userId, int companyId);
        public Task AddCommentAsync(TicketComment ticketComment);
		public Task AssignTicketAsync(int ticketId, string userId);
	}
}
