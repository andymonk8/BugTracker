using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BugTracker.Data;
using BugTracker.Models;
using BugTracker.Models.ViewModels;
using BugTracker.Extensions;
using BugTracker.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace BugTracker.Controllers
{
	[Authorize(Roles = "Admin")]
	public class CompaniesController : Controller
	{
		private readonly ApplicationDbContext _context;
		private readonly IBTCompanyService _companyService;
		private readonly IBTRolesService _rolesService;
		private readonly UserManager<BTUser> _userManager;


		public CompaniesController(ApplicationDbContext context,
								   IBTCompanyService companyService,
								   IBTRolesService rolesService,
								   UserManager<BTUser> userManager)
		{
			_context = context;
			_companyService = companyService;
			_rolesService = rolesService;
			_userManager = userManager;
		}



		// GET: Companies/Details/5
		public async Task<IActionResult> Details(int? id)
		{
			if (id == null || _context.Companies == null)
			{
				return NotFound();
			}

			var company = await _context.Companies
				.FirstOrDefaultAsync(m => m.Id == id);
			if (company == null)
			{
				return NotFound();
			}

			return View(company);
		}

		// GET
		[HttpGet]
		public async Task<IActionResult> ManageUserRoles()
		{
			// 1 - Add an instance of the ViewMOdel as a List (model)
			List<ManageUserRolesViewModel> model = new();

			// 2 - Get CompanyId
			int companyId = User.Identity!.GetCompanyId();

			//3 - Get all company users
			List<BTUser> members = await _companyService.GetMembersAsync(companyId);


			// 4 - Loop over the users to populate the ViewModel
			// - instantiate single ViewModel
			// - user _rolesService
			// - Create multiselect
			// - viewmodel to model
			string btUserId = _userManager.GetUserId(User);

			foreach (BTUser member in members)
			{
				if (string.Compare(btUserId, member.Id) != 0)
				{
					ManageUserRolesViewModel viewModel = new();

					IEnumerable<string> currentRoles = await _rolesService.GetUserRolesAsync(member);

					viewModel.BTUser = member;
					viewModel.Roles = new MultiSelectList(await _rolesService.GetRolesAsync(), "Name", "Name", currentRoles);

					model.Add(viewModel);
				}
			}

			// 5 - Return the model to the View
			return View(model);
		}

		// POST
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ManageUserRoles(ManageUserRolesViewModel viewModel)
		{
			// 1 - Get the company Id
			int companyId = User.Identity!.GetCompanyId();

            // 2 - Instantiate the BTUser
            BTUser? btUser = (await _companyService.GetMembersAsync(companyId)).FirstOrDefault(m => m.Id == viewModel.BTUser!.Id);

			// 3 - Get Roles for the User
			IEnumerable<string> currentRoles = await _rolesService.GetUserRolesAsync(btUser!);

			// 4 - Get Selected Role(s) for the User
			string? selectedRole = viewModel.SelectedRoles!.FirstOrDefault();

			// 5 - Remove current role(s) and Add new role
			if (!string.IsNullOrEmpty(selectedRole))
			{
				if (await _rolesService.RemoveUserFromRolesAsync(btUser!, currentRoles))
				{
					await _rolesService.AddUserToRoleAsync(btUser!, selectedRole);
				}
			}

			// 6 - Navigate
			return RedirectToAction(nameof(ManageUserRoles));

        }
	}
}
