using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BugTracker.Data;
using BugTracker.Models;
using BugTracker.Helper;
using BugTracker.Extensions;
using BugTracker.Services.Interfaces;
using BugTracker.Models.Enums;

using System.IO;
using BugTracker.Services;
using BugTracker.Models.ViewModels;

namespace BugTracker.Controllers
{
    [Authorize]
    public class TicketsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IBTProjectService _projectService;
        private readonly IBTTicketService _ticketService;
        private readonly IBTFileService _fileService;
        private readonly UserManager<BTUser> _userManager;
		private readonly IBTTicketHistoryService _historyService;
        private readonly IBTRolesService _rolesService;
        private readonly IBTNotificationService _notificationService;

        public TicketsController(ApplicationDbContext context,
                                IBTProjectService projectService,
                                IBTTicketService ticketService,
                                IBTFileService fileService,
                                UserManager<BTUser> userManager,
                                IBTTicketHistoryService historyService,
                                IBTRolesService rolesService,
                                IBTNotificationService notificationService)
        {
            _context = context;
            _projectService = projectService;
            _ticketService = ticketService;
            _fileService = fileService;
            _userManager = userManager;
            _historyService = historyService;
            _rolesService = rolesService;
            _notificationService = notificationService;
        }

        // GET: Tickets
        public async Task<IActionResult> AllTickets()
        {

            //int companyId = (await _userManager.GetUserAsync(User)).CompanyId;

            int companyId = User.Identity!.GetCompanyId();

            List<Ticket> tickets = await _ticketService.GetAllTicketsByCompanyIdAsync(companyId);

            return View(tickets);
        }

        // GET: Tickets/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            int companyId = User.Identity!.GetCompanyId();

            if (id == null || _context.Tickets == null)
            {
                return NotFound();
            }

            var ticket = await _ticketService.GetTicketByIdAsync(id.Value, companyId);
            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // GET: Tickets/Create
        public async Task<IActionResult> Create()
        {
            BTUser btUser = await _userManager.GetUserAsync(User);
            List<Project>? projects = new();

            if (User.IsInRole(nameof(BTRoles.Admin)))
            {
                projects = await _projectService.GetAllProjectsByCompanyIdAsync(btUser.CompanyId);
            }
            else
            {
                projects = await _projectService.GetUserProjectsAsync(btUser.Id);
            }

            //(Was " ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Description"); ")
            //ViewData["DeveloperUserId"] = new SelectList(_context.Users, "Id", "Id");
            ViewData["ProjectId"] = new SelectList(projects, "Id", "Name");
            //ViewData["SubmitterUserId"] = new SelectList(_context.Users, "Id", "Id");
            //(Was "ViewData["TicketPriorityId"] = new SelectList(_context.TicketPriorities, "Id", "Name"); ")
            ViewData["TicketPriorityId"] = new SelectList(await _ticketService.GetTicketPrioritiesAync(), "Id", "Name");
            //ViewData["TicketStatusId"] = new SelectList(_context.TicketStatuses, "Id", "Name");
            // Was (" ViewData["TicketTypeId"] = new SelectList(_context.TicketTypes, "Id", "Name"); ")
            ViewData["TicketTypeId"] = new SelectList(await _ticketService.GetTicketTypesAsync(), "Id", "Name");

            return View();
        }

        // POST: Tickets/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,DateCreated,LastUpdated,Archived,ArchivedByProject,ProjectId,TicketTypeId,TicketStatusId,TicketPriorityId,DeveloperUserId,SubmitterUserId")] Ticket ticket)
        {
            BTUser btUser = await _userManager.GetUserAsync(User);
            ModelState.Remove("SubmitterUserId");
            if (ModelState.IsValid)
            {
                string userId = _userManager.GetUserId(User);
                // created date
                ticket.DateCreated = PostgresDate.Format(DateTime.Now);
                // submitter / owner id
                ticket.SubmitterUserId = userId;
                // set status id = "new"
                ticket.TicketStatusId = (await _context.TicketStatuses.FirstOrDefaultAsync(s => s.Name == nameof(BTTicketStatuses.New)))!.Id;


                // call the ticket service
                await _ticketService.AddTicketAsync(ticket);


                // Add History record
                int companyId = User.Identity!.GetCompanyId();
                Ticket newTicket = await _ticketService.GetTicketAsNoTrackingAsync(ticket.Id, companyId);

                await _historyService.AddHistoryAsync(null!, newTicket, userId);

                // Notify!!
                BTUser? projectManager = await _projectService.GetProjectManagerAsync(ticket.ProjectId)!;

                Notification notification = new()
                {
                    NotificationTypeId = (await _context.NotificationTypes.FirstOrDefaultAsync(n => n.Name == nameof(BTNotificationType.Ticket)))!.Id,
                    TicketId = ticket.Id,
                    Title = "New Ticket Added",
                    Message = $"New Ticket : {ticket.Title}, was created by {btUser.FullName}",
                    DateCreated = PostgresDate.Format(DateTime.Now),
                    SenderId = userId,
                    RecipientId = projectManager?.Id
                };

                if (projectManager != null)
                {
                    await _notificationService.AddNotificationAsync(notification);
                    await _notificationService.SendEmailNotificationAsync(notification, $"New Ticket Added for Project: {ticket.Project!.Name}");
                }
                else
                {
                    await _notificationService.AdminNotificationAsync(notification, companyId);
                    await _notificationService.SendAdminEmailNotificationAsync(notification, $"New Ticket Added for Project: {ticket.Project!.Name}", companyId);
                }


                return RedirectToAction(nameof(AllTickets));
                
                //_context.Add(ticket);
                //await _context.SaveChangesAsync();
                //return RedirectToAction(nameof(Index));
            }

            
            List<Project>? projects = new();

            if (User.IsInRole(nameof(BTRoles.Admin)))
            {
                projects = await _projectService.GetAllProjectsByCompanyIdAsync(btUser.CompanyId);
            }
            else
            {
                projects = await _projectService.GetUserProjectsAsync(btUser.Id);
            }


            ViewData["ProjectId"] = new SelectList(projects, "Id", "Name");
            ViewData["TicketPriorityId"] = new SelectList(await _ticketService.GetTicketPrioritiesAync(), "Id", "Name", ticket.TicketPriorityId);
            ViewData["TicketTypeId"] = new SelectList(await _ticketService.GetTicketTypesAsync(), "Id", "Name", ticket.TicketTypeId);
            return View(ticket);
        }

        // GET: Tickets/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            int companyId = User.Identity!.GetCompanyId();


            // call the ticket service
            // Was (" var ticket = await _context.Tickets.FindAsync(id); " ?!)

            
            var ticket = await _ticketService.GetTicketByIdAsync(id.Value,companyId);

            if (ticket == null)
            {
                return NotFound();
            }

            //ViewData["DeveloperUserId"] = new SelectList(_context.Users, "Id", "Id", ticket.DeveloperUserId);
            //ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Description", ticket.ProjectId);
            //ViewData["SubmitterUserId"] = new SelectList(_context.Users, "Id", "Id", ticket.SubmitterUserId);

            //(" ViewData["TicketPriorityId"] = new SelectList(_context.TicketPriorities, "Id", "Name", ticket.TicketPriorityId); ")
            // (Was " ViewData["TicketStatusId"] = new SelectList(_context.TicketStatuses, "Id", "Name", ticket.TicketStatusId); ")
            // (Was " ViewData["TicketTypeId"] = new SelectList(_context.TicketTypes, "Id", "Name", ticket.TicketTypeId); ")

            ViewData["TicketPriorityId"] = new SelectList(await _ticketService.GetTicketPrioritiesAync(), "Id", "Name", ticket.TicketPriorityId);
            ViewData["TicketStatusId"] = new SelectList(await _ticketService.GetTicketStatusesAsync(), "Id", "Name", ticket.TicketStatusId);
            ViewData["TicketTypeId"] = new SelectList(await _ticketService.GetTicketTypesAsync(), "Id", "Name", ticket.TicketTypeId);

            return View(ticket);
        }

        // POST: Tickets/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,DateCreated,LastUpdated,Archived,ArchivedByProject,ProjectId,TicketTypeId,TicketStatusId,TicketPriorityId,DeveloperUserId,SubmitterUserId")] Ticket ticket)
        {
            if (id != ticket.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {

                int companyId = User.Identity!.GetCompanyId();
                string userId = _userManager.GetUserId(User);
                Ticket? oldTicket = await _ticketService.GetTicketAsNoTrackingAsync(ticket.Id, companyId);

                try
                {
                    
                    // format dates
                    ticket.DateCreated = PostgresDate.Format(ticket.DateCreated);

                    ticket.LastUpdated = PostgresDate.Format(DateTime.UtcNow);

                    await _ticketService.UpdateTicketAsync(ticket);

                    //_context.Update(ticket);
                    //await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    // (" if (!TicketExists(ticket.Id)) ")
                    if (!await TicketExists(ticket.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

				// Add History
				Ticket? newTicket = await _ticketService.GetTicketAsNoTrackingAsync(ticket.Id, companyId);
                await _historyService.AddHistoryAsync(oldTicket, newTicket, userId);

                // Notify!!
                BTUser btUser = await _userManager.GetUserAsync(User);
                BTUser? projectManager = await _projectService.GetProjectManagerAsync(ticket.ProjectId)!;

                Notification notification = new()
                {
                    NotificationTypeId = (await _context.NotificationTypes.FirstOrDefaultAsync(n => n.Name == nameof(BTNotificationType.Ticket)))!.Id,
                    TicketId = ticket.Id,
                    Title = "Ticket Updated",
                    Message = $"Ticket : {ticket.Title}, was edited by {btUser.FullName}",
                    DateCreated = PostgresDate.Format(DateTime.Now),
                    SenderId = userId,
                    RecipientId = projectManager?.Id
                };

                if (projectManager != null)
                {
                    await _notificationService.AddNotificationAsync(notification);
                    await _notificationService.SendEmailNotificationAsync(notification, $"New Ticket Edited for Project: {ticket.Project!.Name}");
                }
                else
                {
                    await _notificationService.AdminNotificationAsync(notification, companyId);
                    await _notificationService.SendAdminEmailNotificationAsync(notification, $"New Ticket Edited for Project: {ticket.Project!.Name}", companyId);
                }


                return RedirectToAction(nameof(AllTickets));
            }
            //ViewData["DeveloperUserId"] = new SelectList(_context.Users, "Id", "Id", ticket.DeveloperUserId);
            //ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Description", ticket.ProjectId);
            //ViewData["SubmitterUserId"] = new SelectList(_context.Users, "Id", "Id", ticket.SubmitterUserId);

            // (Was " ViewData["TicketPriorityId"] = new SelectList(_context.TicketPriorities, "Id", "Name", ticket.TicketPriorityId); ")
            // (Was " ViewData["TicketStatusId"] = new SelectList(_context.TicketStatuses, "Id", "Name", ticket.TicketStatusId); ")
            // (Was " ViewData["TicketTypeId"] = new SelectList(_context.TicketTypes, "Id", "Name", ticket.TicketTypeId); ")

            ViewData["TicketPriorityId"] = new SelectList(await _ticketService.GetTicketPrioritiesAync(), "Id", "Name", ticket.TicketPriorityId);
            ViewData["TicketStatusId"] = new SelectList(await _ticketService.GetTicketStatusesAsync(), "Id", "Name", ticket.TicketStatusId);
            ViewData["TicketTypeId"] = new SelectList(await _ticketService.GetTicketTypesAsync(), "Id", "Name", ticket.TicketTypeId);


            return View(ticket);
        }

        // GET: Tickets/Delete/5
        public async Task<IActionResult> Archive(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            int companyId = User.Identity!.GetCompanyId();

            Ticket ticket = await _ticketService.GetTicketByIdAsync(id.Value, companyId);
            
            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // POST: Tickets/Delete/5
        [HttpPost, ActionName("Archive")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ArchivedConfirmed(int id)
        {
            if (id == 0)
            {
                return Problem("Entity set 'ApplicationDbContext.Tickets'  is null.");
            }

            int companyId = User.Identity!.GetCompanyId();

            Ticket ticket = await _ticketService.GetTicketByIdAsync(id, companyId);

            if (ticket != null)
            {
                await _ticketService.ArchiveTicketAsync(ticket);
            }
            // Doesn't / Does Not Have (" await _context.SaveChangesAsync(); ")
            //await _context.SaveChangesAsync();
            return RedirectToAction(nameof(AllTickets));
        }

		// GET: Archive Projects
		[Authorize(Roles = "Admin, ProjectManager")]
		public async Task<IActionResult> ArchivedTickets()
		{
			int companyId = User.Identity!.GetCompanyId();


			List<Ticket> tickets = (await _ticketService.GetAllTicketsByCompanyIdAsync(companyId)).Where(p => p.Archived == true).ToList();

			return View(tickets);
		}

		// GET: Tickets/Delete/5
		public async Task<IActionResult> Restore(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            int companyId = User.Identity!.GetCompanyId();

            Ticket ticket = await _ticketService.GetTicketByIdAsync(id.Value, companyId);
            
            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // POST: Tickets/Restore/5
        [HttpPost, ActionName("Restore")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreConfirmed(int id)
        {
            if (id == 0)
            {
                return Problem("Entity set 'ApplicationDbContext.Tickets'  is null.");
            }

            int companyId = User.Identity!.GetCompanyId();

            Ticket ticket = await _ticketService.GetTicketByIdAsync(id, companyId);

            if (ticket != null)
            {
                await _ticketService.RestoreTicketAsync(ticket);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(AllTickets));
        }

        private async Task<bool> TicketExists(int id)
        {
            //return _context.Tickets.Any(e => e.Id == id);

            int companyId = User.Identity!.GetCompanyId();

            return (await _ticketService.GetAllTicketsByCompanyIdAsync(companyId)).Any(t => t.Id == id);
        }

        [HttpGet]
        [Authorize(Roles = "Admin, ProjectManager")]
        public async Task<IActionResult> UnassignedTickets()
        {
            int companyId = User.Identity!.GetCompanyId();
            string btUserId = _userManager.GetUserId(User);

            // (" if (User.IsInRole(nameof(BTRoles.Admin, BTRoles.ProjectManager))) ")

            List<Ticket> tickets = await _ticketService.GetUnassignedTicketsAsync(companyId);
            if (User.IsInRole(nameof(BTRoles.Admin)))
            {
                return View(tickets);
            }
            else
            {
                List<Ticket> unassignedTickets = new();
                foreach (Ticket ticket in tickets)
                {
                    if (await _projectService.IsAssignedProjectManagerAsync(btUserId, ticket.ProjectId))
                        unassignedTickets.Add(ticket);
                }
                return RedirectToAction(nameof(UnassignedTickets));
            }
        }

		[HttpGet]
		public async Task<IActionResult> MyTickets()
		{
			int companyId = User.Identity!.GetCompanyId();
			
            string userId = _userManager.GetUserId(User);

			List<Ticket> tickets = await _ticketService.GetTicketByUserIdAsync(userId, companyId);


			return View(tickets);
		}


		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> AddTicketAttachment([Bind("Id,FormFile,Description,TicketId")] TicketAttachment ticketAttachment)
		{
			ModelState.Remove("UserId");
			string statusMessage;

			if (ModelState.IsValid && ticketAttachment.FormFile != null)
			{
				ticketAttachment.FileData = await _fileService.ConvertFileToByteArrayAsync(ticketAttachment.FormFile);
				ticketAttachment.FileName = ticketAttachment.FormFile.FileName;
				ticketAttachment.FileType = ticketAttachment.FormFile.ContentType;

				ticketAttachment.DateCreated = DateTime.UtcNow;
				ticketAttachment.UserId = _userManager.GetUserId(User);

				await _ticketService.AddTicketAttachmentAsync(ticketAttachment);
                await _historyService.AddHistoryAsync(ticketAttachment.TicketId, nameof(TicketAttachment), ticketAttachment.UserId);
				statusMessage = "Success: New attachment added to Ticket.";
			}
			else
			{
				statusMessage = "Error: Invalid data.";

			}


            return RedirectToAction("Details", new { id = ticketAttachment.TicketId, message = statusMessage });
		}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTicketComment([Bind("Id,TicketId,Comment")] TicketComment ticketComment)
        {
            ModelState.Remove("UserId");

            if (ModelState.IsValid)
            {
                try
                {
                    ticketComment.UserId = _userManager.GetUserId(User);
                    ticketComment.DateCreated = DateTime.UtcNow;

                    await _ticketService.AddCommentAsync(ticketComment);

                    await _historyService.AddHistoryAsync(ticketComment.TicketId, nameof(TicketComment), ticketComment.UserId);

                }
                catch (Exception)
                {
                    throw;
                }
            }

			return RedirectToAction("Details", new { id = ticketComment.TicketId });
			//return RedirectToAction("Details");
			//return RedirectToAction(nameof(Details));
		}

		public async Task<IActionResult> ShowFile(int id)
		{
			TicketAttachment ticketAttachment = await _ticketService.GetTicketAttachmentByIdAsync(id);
			string fileName = ticketAttachment.FileName;
			byte[] fileData = ticketAttachment.FileData;
			string ext = Path.GetExtension(fileName).Replace(".", "");

			Response.Headers.Add("Content-Disposition", $"inline; filename={fileName}");
			return File(fileData, $"application/{ext}");
		}


		[HttpGet]
		[Authorize(Roles = "Admin, ProjectManager")]
		public async Task<IActionResult> AssignDeveloper(int? ticketId)
		{
			int companyId = User.Identity!.GetCompanyId();

			AssignDeveloperViewModel viewModel = new();

			viewModel.Ticket = await _ticketService.GetTicketByIdAsync(ticketId.Value, companyId);


			List<BTUser> developers = await _rolesService.GetUsersInRoleAsync(nameof(BTRoles.Developer), companyId);
			

			viewModel.DeveloperList = new MultiSelectList(developers, "Id", "FullName");

			return View(viewModel);
		}

        
        [HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> AssignDeveloper(AssignDeveloperViewModel viewModel, int companyId, string developer)
		{
            // 12/12/2022
            // (" if (viewModel.Ticket?.Id != null) ")

            if (viewModel.DeveloperId != null)
			{
                // 12/12/2022
                BTUser btUser = await _userManager.GetUserAsync(User);
                Ticket? oldTicket = await _ticketService.GetTicketByIdAsync(viewModel.Ticket!.Id, companyId);


                try
                {
                    await _ticketService.AssignTicketAsync(viewModel.Ticket.Id, viewModel.DeveloperId);
                }
                catch (Exception)
                {

                    throw;
                }

                // 12/12/2022 (Code Was Placed Here?!)
                //BTUser btUser = await _userManager.GetUserAsync(User);

                Notification notification = new()
                {
                    NotificationTypeId = (await _context.NotificationTypes.FirstOrDefaultAsync(n => n.Name == nameof(BTNotificationType.Ticket)))!.Id,
                    TicketId = viewModel.Ticket.Id,
                    Title = "Ticket Assignment",
                    Message = $"Ticket : {viewModel.Ticket.Title}, was assigned by {btUser.FullName}",
                    DateCreated = PostgresDate.Format(DateTime.Now),
                    SenderId = btUser.Id,
                    RecipientId = viewModel.DeveloperId
                };

                // Add and Send Notification / &
                await _notificationService.AddNotificationAsync(notification);
                await _notificationService.SendEmailNotificationAsync(notification, "Ticket Assignment");


                return RedirectToAction(nameof(Details), new { id = viewModel.Ticket?.Id });
			}

			return RedirectToAction(nameof(AssignDeveloper), new { ticketId = viewModel.Ticket!.Id });

		}



	}
}
