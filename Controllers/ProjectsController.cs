using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BugTracker.Data;
using BugTracker.Models;

using BugTracker.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using X.PagedList;
using BugTracker.Services;
using BugTracker.Helper;
using BugTracker.Extensions;

using BugTracker.Models.Enums;
using BugTracker.Models.ViewModels;

namespace BugTracker.Controllers
{
    [Authorize]
    public class ProjectsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IBTProjectService _projectService;
        private readonly IBTTicketService _ticketService;
        private readonly IBTFileService _fileService;
        private readonly IBTRolesService _rolesService;
        private readonly UserManager<BTUser> _userManager;

        public ProjectsController(ApplicationDbContext context,
                                  IBTProjectService projectService,
                                  IBTTicketService ticketService,
                                  IBTFileService fileService,
                                  IBTRolesService rolesService,
                                  UserManager<BTUser> userManager)
        {
            _context = context;
            _projectService = projectService;
            _ticketService = ticketService;
            _fileService = fileService;
            _rolesService = rolesService;
            _userManager = userManager;
        }

        // GET: Projects
        public async Task<IActionResult> AllProjects()
        {

            int companyId = User.Identity!.GetCompanyId();

            List<Project> projects = await _projectService.GetAllProjectsByCompanyIdAsync(companyId);

            return View(projects);
        }

        // GET: Archive Projects
        [Authorize(Roles = "Admin, ProjectManager")]
        public async Task<IActionResult> ArchivedProjects()
        {
            int companyId = User.Identity!.GetCompanyId();


            List<Project> projects = (await _projectService.GetAllProjectsByCompanyIdAsync(companyId)).Where(p => p.Archived == true).ToList();

            return View(projects);
        }

        [HttpGet]
        [Authorize(Roles = nameof(BTRoles.Admin))]
        public async Task<IActionResult> AssignPM(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            int companyId = User.Identity!.GetCompanyId();

            List<BTUser> projectManagers = await _rolesService.GetUsersInRoleAsync(nameof(BTRoles.ProjectManager), User.Identity!.GetCompanyId());
            BTUser? currentPM = await _projectService.GetProjectManagerAsync(id.Value);

            AssignPMViewModel viewModel = new()
            {
                Project = await _projectService.GetProjectByIdAsync(id.Value, companyId),
                PMList = new SelectList(projectManagers, "Id", "FullName", currentPM?.Id),
                PMId = currentPM?.Id
            };

            return View(viewModel);
        }

        [HttpPost]
        [Authorize(Roles = nameof(BTRoles.Admin))]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignPM(AssignPMViewModel viewModel)
        {
            if (viewModel.Project?.Id != null)
            {
                if (!string.IsNullOrEmpty(viewModel.PMId))
                {
                    //BTUser newPM = await _userManager.FindByIdAsync(viewModel.PMId);
                    // (" newPM ") as Parameter for Below?! If Needed?! Etc.?!

                    await _projectService.AddProjectManagerAsync(viewModel.PMId, viewModel.Project.Id);
                }
                else
                {
                    await _projectService.RemoveProjectManagerAsync(viewModel.Project.Id);
                }

                return RedirectToAction(nameof(Details), new { id = viewModel.Project?.Id });
            }

            return View(viewModel);
        }

        [HttpGet]
        [Authorize(Roles = "Admin, ProjectManager")]
        public async Task<IActionResult> AssignProjectMembers(int? projectId)
        {
            // 12/11/2022
            //if (projectId == null)
            //{
            //    return NotFound();
            //}

            int companyId = User.Identity!.GetCompanyId();

            ProjectMembersViewModel viewModel = new();

            viewModel.Project = await _projectService.GetProjectByIdAsync(projectId.Value, companyId);

            List<BTUser> developers = await _rolesService.GetUsersInRoleAsync(nameof(BTRoles.Developer), companyId);
            List<BTUser> submitters = await _rolesService.GetUsersInRoleAsync(nameof(BTRoles.Submitter), companyId);

            List<BTUser> teamMembers = developers.Concat(submitters).ToList();

            List<string> projectMembers = viewModel.Project.Members.Select(p => p.Id).ToList();

            viewModel.UsersList = new MultiSelectList(teamMembers, "Id", "FullName", projectMembers);

            return View(viewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignProjectMembers(ProjectMembersViewModel viewModel)
        {
            if (viewModel.Project?.Id != null)
            {
                if (!string.IsNullOrEmpty(viewModel.SelectedMembers))
                {
                    //BTUser newPM = await _userManager.FindByIdAsync(viewModel.PMId);
                    // (" newPM ") as Parameter for Below?! If Needed?! Etc.?!

                    BTUser member = await _userManager.FindByIdAsync(viewModel.SelectedMembers);

                    await _projectService.AddMemberToProjectAsync(member, viewModel.Project.Id);
                }

                return RedirectToAction(nameof(Details), new { id = viewModel.Project?.Id });
            }

            // 12/11/2022
			//return View(viewModel);

			return RedirectToAction(nameof(AssignProjectMembers), new { projectId = viewModel.Project!.Id });
		}


		// GET: Users Projects
		[HttpGet]
		public async Task<IActionResult> MyProjects()
		{
			string userId = _userManager.GetUserId(User);
			var projects = await _projectService.GetUserProjectsAsync(userId);

			return View(projects);
		}

		[HttpGet]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> UnassignedProjects()
		{
			int companyId = User.Identity.GetCompanyId();

			List<Project> projects = await _projectService.GetUnassignedProjectsAsync(companyId);

			return View(projects);

		}

		// GET: Projects/Details/5
		public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            //int companyId = (await _userManager.GetUser)

            int companyId = User.Identity!.GetCompanyId();

            var project = await _projectService.GetProjectByIdAsync(id.Value, companyId);

            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // GET: Projects/Create
        [Authorize(Roles = "Admin, ProjectManager")]
        public async Task<IActionResult> Create()
        {

            //ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name");
            //ViewData["ProjectPriorityId"] = new MultiSelectList(await _projectService.GetTagsAsync(), "Id", "Name");
            //return View(new Project());

            // ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name");
            
            ViewData["ProjectPriorityId"] = new SelectList(await _projectService.GetProjectPrioritiesAsync(), "Id", "Name");
            return View(new Project());
        }

        // POST: Projects/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "Admin, ProjectManager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,CompanyId,Name,Description,DateCreated,StartDate,EndDate,ProjectPriorityId,ImageData,ImageType,Archived,ProjectImage")] Project project)
        {
            if (ModelState.IsValid)
            {
                // Get Company Id
                int companyId = User.Identity!.GetCompanyId();
                project.CompanyId = companyId;

                // Set Created Date
                //project.DateCreated = PostgresDate.Format(DateTime.UtcNow);

                project.DateCreated = PostgresDate.Format(project.DateCreated);

                // Determine if an image has been uploaded
                if (project.ProjectImage != null)
                {
                    project.ImageData = await _fileService.ConvertFileToByteArrayAsync(project.ProjectImage);
                    project.ImageType = project.ProjectImage.ContentType;
                }

                // Format start and end dates
                //project.StartDate = PostgresDate.Format(DateTime.UtcNow);
                //project.EndDate = PostgresDate.Format(DateTime.UtcNow);

                project.StartDate = PostgresDate.Format(project.StartDate!.Value);
                project.EndDate = PostgresDate.Format(project.EndDate!.Value);

                // Call Project Service
                await _projectService.AddProjectAsync(project);


                return RedirectToAction(nameof(AllProjects));
            }


            // ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name", project.CompanyId);
            // (Was (" ViewData["ProjectPriorityId"] = new SelectList(_context.ProjectPriorities, "Id", "Id", project.ProjectPriorityId); ")
            ViewData["ProjectPriorityId"] = new SelectList(await _projectService.GetProjectPrioritiesAsync(), "Id", "Id", project.ProjectPriorityId);
            return View(project);
        }

        // GET: Projects/Edit/5
        [Authorize(Roles = "Admin, ProjectManager")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            int companyId = User.Identity!.GetCompanyId();

            var project = await _projectService.GetProjectByIdAsync(id.Value, companyId);

            if (project == null)
            {
                return NotFound();
            }

            //ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name", project.CompanyId);
            ViewData["ProjectPriorityId"] = new SelectList(await _projectService.GetProjectPrioritiesAsync(), "Id", "Name", project.ProjectPriorityId);
            return View(project);
        }

        // POST: Projects/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "Admin, ProjectManager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CompanyId,Name,Description,DateCreated,StartDate,EndDate,ProjectPriorityId,ImageData,ImageType,Archived,ProjectImage")] Project project)
        {
            if (id != project.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    int companyId = User.Identity!.GetCompanyId();
                    project.CompanyId = companyId;

                    //project.DateCreated = DateTime.SpecifyKind(project.DateCreated, DateTimeKind.Utc);

                    project.DateCreated = PostgresDate.Format(project.DateCreated);
                    //project.DateCreated = PostgresDate.Format(DateTime.UtcNow);

                    project.StartDate = PostgresDate.Format(project.StartDate!.Value);
                    project.EndDate = PostgresDate.Format(project.EndDate!.Value);

                    // Gets Red Ramen?!
                    //project.StartDate = PostgresDate.Format(project.StartDate);
                    //project.EndDate = PostgresDate.Format(project.EndDate);

                    //project.StartDate = DateTime.SpecifyKind(project.StartDate.Value, DateTimeKind.Utc);

                    if (project.ProjectImage != null)
                    {
                        project.ImageData = await _fileService.ConvertFileToByteArrayAsync(project.ProjectImage);
                        project.ImageType = project.ProjectImage.ContentType;
                    }

                    //_context.Update(project);
                    //await _context.SaveChangesAsync();
                    //await _projectService.UpdateProjectAsync(project);


                    //return RedirectToAction(nameof(AllProjects));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await ProjectExists(project.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                //return RedirectToAction(nameof(AllProjects));

                await _projectService.UpdateProjectAsync(project);


                return RedirectToAction(nameof(AllProjects));
            }
            //ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name", project.CompanyId);
            ViewData["ProjectPriorityId"] = new SelectList(await _projectService.GetProjectPrioritiesAsync(), "Id", "Name", project.ProjectPriorityId);
            return View(project);
        }

        // GET: Projects/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Archive(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            int companyId = User.Identity!.GetCompanyId();

            var project = await _projectService.GetProjectByIdAsync(id.Value, companyId);

            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // POST: Projects/Delete/5
        [HttpPost, ActionName("Archive")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ArchiveConfirmed(int id)
        {
            if (id == 0)
            {
                return NotFound();
            }

            int companyId = User.Identity!.GetCompanyId();

            var project = await _projectService.GetProjectByIdAsync(id, companyId);

            if (project != null)
            {
                project.Archived = true;
                await _projectService.UpdateProjectAsync(project);
            }
            
            //await _context.SaveChangesAsync();
            return RedirectToAction(nameof(AllProjects));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Restore(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            int companyId = User.Identity!.GetCompanyId();

            // To Do: Call the service
            var project = await _projectService.GetProjectByIdAsync(id.Value, companyId);

            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // POST: Projects/Delete/5
        [HttpPost, ActionName("Restore")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreConfirmed(int id)
        {
            if (id == 0)
            {
                return NotFound();
            }

            int companyId = User.Identity!.GetCompanyId();

            var project = await _projectService.GetProjectByIdAsync(id, companyId);

            if (project != null)
            {
                project.Archived = false;
                await _projectService.UpdateProjectAsync(project);
            }

            //await _context.SaveChangesAsync();
            return RedirectToAction(nameof(AllProjects));
        }


        private async Task<bool> ProjectExists(int id)
        {
            int companyId = User.Identity!.GetCompanyId();
          return (await _projectService.GetAllProjectsByCompanyIdAsync(companyId)).Any(p => p.Id == id);
        }

    }
}
