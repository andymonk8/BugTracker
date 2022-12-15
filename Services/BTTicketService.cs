using System.ComponentModel.Design;
using BugTracker.Data;
using BugTracker.Models;
using BugTracker.Models.Enums;
using BugTracker.Services.Interfaces;
using Microsoft.Build.Evaluation;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace BugTracker.Services
{
    public class BTTicketService : IBTTicketService
    {
        private readonly ApplicationDbContext _context;
        private readonly IBTRolesService _rolesService;
        private readonly IBTProjectService _projectService;

        public BTTicketService(ApplicationDbContext context,
                                IBTRolesService rolesService,
                                IBTProjectService projectService)
        {
            _context = context;
            _rolesService = rolesService;
            _projectService = projectService;
        }

        public async Task AddCommentAsync(TicketComment ticketComment)
        {
            try
            {
                await _context.AddAsync(ticketComment);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task AddTicketAsync(Ticket ticket)
        {
            try
            {
                await _context.AddAsync(ticket);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task AddTicketAttachmentAsync(TicketAttachment ticketAttachment)
        {
            try
            {
                await _context.AddAsync(ticketAttachment);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task ArchiveTicketAsync(Ticket ticket)
        {
            try
            {
                ticket.Archived = true;
                await UpdateTicketAsync(ticket);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task AssignDeveloperAsync(int ticketId, string userId, int companyId)
        {
            try
            {
                Ticket ticket = await GetTicketByIdAsync(ticketId, companyId);

                ticket.DeveloperUserId = userId;

                _context.Update(ticket);

                //await UpdateTicketAsync(ticket);

                await _context.SaveChangesAsync();

                //BTUser? currentTD = await GetTicketDeveloperAsync(ticketId, companyId);
                //BTUser? selectedTD = await _context.Users.FindAsync(userId);

                //// Remove current PM
                //if (currentTD != null)
                //{
                //    await RemoveTicketDeveloperAsync(ticketId);
                //}


                //// Add new PM
                //try
                //{
                //    await AddMemberToTicketAsync(selectedTD!, ticketId);
                //    return task;
                //}
                //catch (Exception)
                //{

                //    throw;
                //}

            }
            catch (Exception)
            {

                throw;
            }
        }

		public async Task AssignTicketAsync(int ticketId, string userId)
		{
			Ticket ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.Id == ticketId);
			try
			{
				if (ticket != null)
				{
					try
					{
						ticket.DeveloperUserId = userId;
						await _context.SaveChangesAsync();
					}
					catch (Exception)
					{

						throw;
					}
				}
			}
			catch (Exception)
			{

				throw;
			}
		}

		public async Task<List<Ticket>> GetAllTicketsByCompanyIdAsync(int companyId)
        {
            try
            {
                List<Ticket> tickets = new();

                tickets = await _context.Tickets
                                       .Where(t => t.Project!.CompanyId == companyId)
                                       .Include(t => t.Project)
                                            .ThenInclude(p => p.Company)
                                       .Include(t => t.Attachments)
                                       .Include(t => t.Comments)
                                       .Include(t => t.History)
                                       .Include(t => t.DeveloperUser)
                                       .Include(t => t.SubmitterUser)
                                       .Include(t => t.TicketPriority)
                                       .Include(t => t.TicketStatus)
                                       .Include(t => t.TicketType)
                                       .ToListAsync();

                return tickets;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<Ticket> GetTicketAsNoTrackingAsync(int ticketId, int companyId)
        {
            try
            {
                Ticket? ticket = await _context.Tickets
                                               .Include(t => t.Project)
                                                    .ThenInclude(p => p.Company)
                                               .Include(t => t.Attachments)
                                               .Include(t => t.Comments)
                                               .Include(t => t.History)
                                               .Include(t => t.DeveloperUser)
                                               .Include(t => t.SubmitterUser)
                                               .Include(t => t.TicketPriority)
                                               .Include(t => t.TicketStatus)
                                               .Include(t => t.TicketType)
                                               .AsNoTracking()
                                               .FirstOrDefaultAsync(t => t.Id == ticketId && t.Project!.CompanyId == companyId && t.Archived == false);

                return ticket!;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<TicketAttachment> GetTicketAttachmentByIdAsync(int ticketAttachmentId)
        {
            try
            {
                TicketAttachment ticketAttachment = await _context.TicketAttachments
                                                                  .Include(t => t.User)
                                                                  .FirstOrDefaultAsync(t => t.Id == ticketAttachmentId);
                return ticketAttachment;
            }
            catch (Exception)
            {

                throw;
            }
        }

		// .FirstOrDefaultAsync(t => t.Id == ticketId && t.Project!.CompanyId == companyId && t.Archived == false);
		// && t.Project!.CompanyId == companyId
		public async Task<Ticket> GetTicketByIdAsync(int ticketId, int companyId)
        {
            try
            {
                Ticket? ticket = await _context.Tickets
                                               .Include(t => t.Project)
                                                    .ThenInclude(p => p.Company)
                                               .Include(t => t.Attachments)
                                               .Include(t => t.Comments)
                                                .ThenInclude(t => t.User)
                                               .Include(t => t.History)
                                               .Include(t => t.DeveloperUser)
                                               .Include(t => t.SubmitterUser)
                                               .Include(t => t.TicketPriority)
                                               .Include(t => t.TicketStatus)
                                               .Include(t => t.TicketType)
                                               .FirstOrDefaultAsync(t => t.Id == ticketId);

                return ticket!;
            }
            catch (Exception)
            {

                throw;
            }
        }

		public async Task<List<Ticket>> GetTicketByUserIdAsync(string userId, int companyId)
        {
            BTUser? btUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            // List<Ticket>? tickets = new();
            List<Ticket>? tickets = (await _projectService.GetAllProjectsByCompanyIdAsync(companyId)).Where(p => p.Archived == false)
                                                    .SelectMany(p => p.Tickets!).Where(t => t.Archived==false | t.ArchivedByProject==false).ToList();



            try
            {
                if (await _rolesService.IsUserInRoleAsync(btUser!, nameof(BTRoles.Admin)))
                {
                    return tickets;
                }
                else if (await _rolesService.IsUserInRoleAsync(btUser!, nameof(BTRoles.Developer)))
                {
                    return tickets.Where(t => t.DeveloperUserId == userId || t.SubmitterUserId == userId).ToList();
                }
                else if (await _rolesService.IsUserInRoleAsync(btUser!, nameof(BTRoles.Submitter)))
                {
                    return tickets.Where(t => t.SubmitterUserId == userId).ToList();
                }
                else if (await _rolesService.IsUserInRoleAsync(btUser!, nameof(BTRoles.ProjectManager)))
                {
                    //List<Ticket>? projectTickets = (await _projectService.GetUserProjectsAsync(userId))!.SelectMany(t => t.Tickets!).ToList();
                    List<Ticket>? projectTickets = (await _projectService.GetUserProjectsAsync(userId))!.SelectMany(t => t.Tickets!).Where(t => t.Archived | t.ArchivedByProject).ToList();
                    List<Ticket>? submittedTickets = tickets.Where(t => t.SubmitterUserId == userId).ToList();
                    //List<Ticket>? submittedTickets = (await _projectService.GetAllProjectsByCompanyIdAsync(companyId))
                    //.SelectMany(p => p.Tickets!).Where(t => t.SubmitterUserId == userId).ToList();
                    //tickets = projectTickets.Concat(submittedTickets).ToList();
                    return tickets = projectTickets.Concat(submittedTickets).ToList();
                }

                return tickets;
            }
            catch (Exception)
            {

                throw;
            }
        }

		public async Task<BTUser> GetTicketDeveloperAsync(int ticketId, int companyId)
        {
            BTUser developer = new();

            try
            {
                Ticket ticket = (await GetAllTicketsByCompanyIdAsync(companyId)).FirstOrDefault(t => t.Id == ticketId);

                if (ticket?.DeveloperUserId != null)
                {
                    developer = ticket.DeveloperUser;
                }

                return developer;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<IEnumerable<TicketPriority>> GetTicketPrioritiesAync()
        {
            try
            {
                return await _context.TicketPriorities.ToListAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

		public async Task<IEnumerable<TicketStatus>> GetTicketStatusesAsync()
        {
            try
            {
                return await _context.TicketStatuses.ToListAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<IEnumerable<TicketType>> GetTicketTypesAsync()
        {
            try
            {
                return await _context.TicketTypes.ToListAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<Ticket>> GetUnassignedTicketsAsync(int companyId)
        {
            List<Ticket> tickets = new();

            try
            {
                tickets = (await GetAllTicketsByCompanyIdAsync(companyId)).Where(t => string.IsNullOrEmpty(t.DeveloperUserId)).ToList();
                return tickets;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task RestoreTicketAsync(Ticket ticket)
        {
            try
            {
                ticket.Archived = false;
                await UpdateTicketAsync(ticket);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task UpdateTicketAsync(Ticket ticket)
        {
            try
            {
                _context.Update(ticket);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
