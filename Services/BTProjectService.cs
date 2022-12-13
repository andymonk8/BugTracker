using BugTracker.Data;
using BugTracker.Models;
using BugTracker.Models.Enums;
using BugTracker.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using X.PagedList;


namespace BugTracker.Services
{
    public class BTProjectService : IBTProjectService
    {
        private readonly ApplicationDbContext _context;
        private readonly IBTRolesService _rolesService;

        public BTProjectService(ApplicationDbContext context, 
                                IBTRolesService rolesService)
        {
            _context = context;
            _rolesService = rolesService;
        }
        
        public async Task<bool> AddMemberToProjectAsync(BTUser member, int projectId)
        {
            try
            {
                Project? project = await GetProjectByIdAsync(projectId, member.CompanyId);

                bool IsOnProject = project.Members.Any(m => m.Id == member.Id);

                if(!IsOnProject)
                {
                    project.Members.Add(member);
                    await _context.SaveChangesAsync();
                    return true;
                }

                return false;

            }
            catch (Exception)
            {

                throw;
            }
        }
        
        public async Task AddProjectAsync(Project project)
        {
            try
            {
                _context.Add(project);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }
        
        public async Task<bool> AddProjectManagerAsync(string userId, int projectId)
        {
            try
            {
                BTUser? currentPM = await GetProjectManagerAsync(projectId);
                BTUser? selectedPM = await _context.Users.FindAsync(userId);

                // Remove current PM
                if(currentPM != null)
                {
                    await RemoveProjectManagerAsync(projectId);
                }


                // Add new PM
                try
                {
                    await AddMemberToProjectAsync(selectedPM!, projectId);
                    return true;
                }
                catch (Exception)
                {

                    throw;
                }

            }
            catch (Exception)
            {

                throw;
            }
        }
        
        public async Task ArchiveProjectAsync(int projectId, Project project)
        {
            // projectId / project Id?!
            try
            {
                project.Archived = true;
                await UpdateProjectAsync(project);

                foreach (Ticket ticket in project.Tickets)
                {
                    ticket.ArchivedByProject = true;
                    _context.Update(ticket);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

		public async Task<List<BTUser>> GetAllProjectMembersExceptPMAsync(int projectId, BTUser memeber)
		{
			try
			{
				List<BTUser> developers = await GetProjectMembersByRoleAsync(projectId, BTRoles.Developer.ToString());
				List<BTUser> submitters = await GetProjectMembersByRoleAsync(projectId, BTRoles.Submitter.ToString());
				List<BTUser> admins = await GetProjectMembersByRoleAsync(projectId, BTRoles.Admin.ToString());

				List<BTUser> teamMembers = developers.Concat(submitters).Concat(admins).ToList();

				return teamMembers;
			}
			catch (Exception)
			{

				throw;
			}
		}

		// (" .Where(p => p.Archived == false) ")
		public async Task<List<Project>> GetAllProjectsByCompanyIdAsync(int companyId)
        {
            try
            {
                List<Project> projects = new();

                projects = await _context.Projects.Where(p => p.CompanyId == companyId)
                                             .Include(p => p.Company)
                                             .Include(p => p.Members)
                                             .Include(p => p.Tickets)
                                                .ThenInclude(t => t.Attachments)
                                             .Include(p => p.Tickets)
                                                .ThenInclude(t => t.Comments)
                                             .Include(p => p.Tickets)
                                                .ThenInclude(t => t.History)
                                             .Include(p => p.Tickets)
                                                .ThenInclude(t => t.DeveloperUser)
                                             .Include(p => p.Tickets)
                                                .ThenInclude(t => t.SubmitterUser)
                                             .Include(p => p.Tickets)
                                                .ThenInclude(t => t.TicketStatus)
                                             .Include(p => p.Tickets)
                                                .ThenInclude(t => t.TicketPriority)
                                             .Include(p => p.Tickets)
                                                .ThenInclude(t => t.TicketType)
                                             .Include(p => p.ProjectPriority)
                                             .ToListAsync();

                return projects;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<Project>?> GetAllProjectsByPriorityAsync(int companyId, int priority, string priorityName)
        {
            try
            {
                List<Project> projects = await GetAllProjectsByCompanyIdAsync(companyId);
                int priorityId = await LookUpProjectPriorityId(priorityName);

                return projects.Where(p => p.ProjectPriorityId == priorityId).ToList();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<Project>> GetArchivedProjectsByCompanyIdAsync(int companyId)
        {
            List<Project> projects = (await GetAllProjectsByCompanyIdAsync(companyId)).Where(p => p.Archived == true).ToList();
            return projects;

            //    IPagedList<BlogPost> blogPostPage = (await _blogPostService.GetAllBlogPostsAsync())
            //                                    .Where(b => b.IsDeleted == true)
            //                                    .ToPagedList(page, pageSize);

            //    return View(blogPostPage);
        }

        public async Task<Project> GetProjectByIdAsync(int projectId, int companyId)
        {
            try
            {
                Project? project = new();

                project = await _context.Projects
                                        .Include(p => p.Company)
                                        .Include(p => p.Members)
                                        .Include(p => p.Tickets)
                                            .ThenInclude(t => t.Comments)
                                        .Include(p => p.Tickets)
                                            .ThenInclude(t => t.Attachments)
                                        .Include(p => p.Tickets)
                                            .ThenInclude(t => t.History)
                                        .Include(p => p.Tickets)
                                            .ThenInclude(t => t.TicketPriority)
                                        .Include(p => p.Tickets)
                                            .ThenInclude(t => t.TicketStatus)
                                        .Include(p => p.Tickets)
                                            .ThenInclude(t => t.TicketType)
                                        .Include(p => p.ProjectPriority)
                                        .FirstOrDefaultAsync(p => p.Id == projectId && p.CompanyId == companyId);

                return project!;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<BTUser> GetProjectManagerAsync(int projectId)
        {
            try
            {
                Project? project = _context.Projects.Include(p=>p.Members).FirstOrDefault(p => p.Id == projectId);

                foreach(BTUser member in project!.Members)
                {
                    if (await _rolesService.IsUserInRoleAsync(member,nameof(BTRoles.ProjectManager)))
                    {
                        return member;
                    }
                }

                return null!;
            }
            catch (Exception)
            {

                throw;
            }
        }

		public async Task<List<BTUser>> GetProjectMembersByRoleAsync(int projectId, string role)
		{
			try
			{
				Project? project = await _context.Projects
										.Include(p => p.Members)
										.FirstOrDefaultAsync(p => p.Id == projectId);

				List<BTUser> members = new();

				foreach (var user in project.Members)
				{
					if (await _rolesService.IsUserInRoleAsync(user, role))
					{
						members.Add(user);
					}
				}

				return members;

			}
			catch (Exception)
			{

				throw;
			}
		}

		public async Task<IEnumerable<ProjectPriority>> GetProjectPrioritiesAsync()
        {
            try
            {
                return await _context.ProjectPriorities.ToListAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<Project>?> GetUserProjectsAsync(string userId)
        {
            try
            {
                List<Project>? projects = (await _context.Users
                                                         .Include(u => u.Projects)
                                                             .ThenInclude(p => p.ProjectPriority)
                                                         .Include(u => u.Projects)
                                                             .ThenInclude(p => p.Members)
                                                         .Include(u => u.Projects)
                                                             .ThenInclude(p => p.Tickets)
                                                                 .ThenInclude(u => u.TicketPriority)
                                                        .Include(u => u.Projects)
                                                             .ThenInclude(p => p.Tickets)
                                                                 .ThenInclude(u => u.TicketType)
                                                        .Include(u => u.Projects)
                                                             .ThenInclude(p => p.Tickets)
                                                                 .ThenInclude(u => u.TicketStatus)
                                                        .Include(u => u.Projects)
                                                             .ThenInclude(p => p.Tickets)
                                                                 .ThenInclude(u => u.Comments)
                                                        .Include(u => u.Projects)
                                                             .ThenInclude(p => p.Tickets)
                                                                 .ThenInclude(u => u.Attachments)
                                                        .Include(u => u.Projects)
                                                             .ThenInclude(p => p.Tickets)
                                                                 .ThenInclude(u => u.History)
                                                        .Include(u => u.Projects)
                                                             .ThenInclude(p => p.Tickets)
                                                                 .ThenInclude(u => u.SubmitterUser)
                                                        .Include(u => u.Projects)
                                                             .ThenInclude(p => p.Tickets)
                                                                 .ThenInclude(u => u.DeveloperUser)
                                                         .FirstOrDefaultAsync(u => u.Id == userId))?
                                                         .Projects.Where(p=>p.Archived == false)
                                                         .ToList();

                return projects;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<bool> IsAssignedProjectManagerAsync(string userId, int projectId)
        {
            try
            {
                string? projectManagerId = (await GetProjectManagerAsync(projectId))?.Id;

                if (projectManagerId == userId)
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<int> LookUpProjectPriorityId(string priorityName)
        {
            try
            {
                int priorityId = (await _context.ProjectPriorities.FirstOrDefaultAsync(p => p.Name == priorityName)).Id;
                return priorityId;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<bool> RemoveMemberFromProjectAsync(BTUser member, int projectId, int companyId)
        {
            try
            {
                Project? project = await GetProjectByIdAsync(projectId, member.CompanyId);

                bool IsOnProject = project.Members.Any(m => m.Id == member.Id);

                if (IsOnProject)
                {
                    project.Members.Remove(member);
                    await _context.SaveChangesAsync();
                    return true;
                }

                return false;

            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task RemoveProjectManagerAsync(int projectId)
        {
            BTUser? currentPM = await GetProjectManagerAsync(projectId);

            if (currentPM != null)
            {
                await RemoveMemberFromProjectAsync(currentPM, projectId, currentPM.CompanyId);
                await _context.SaveChangesAsync();
            }
        }
        
        public async Task RestoreProjectAsync(int projectId, Project project)
        {
            // projectId / project Id?!
            try
            {
                project.Archived = false;
                await UpdateProjectAsync(project);

                foreach (Ticket ticket in project.Tickets)
                {
                    ticket.ArchivedByProject = false;
                    _context.Update(ticket);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
        
        public async Task UpdateProjectAsync(Project project)
        {
            try
            {
                _context.Update(project);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}

