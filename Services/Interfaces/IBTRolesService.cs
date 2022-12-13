using BugTracker.Models;
using Microsoft.AspNetCore.Identity;

namespace BugTracker.Services.Interfaces
{
    public interface IBTRolesService
    {
        public Task<bool> AddUserToRoleAsync(BTUser user, string roleName);
        /// <summary>
        /// Get All Roles
        /// </summary>
        /// <returns></returns>
        public Task<List<IdentityRole>> GetRolesAsync();

        /// <summary>
        /// Get the role(s) for the provided BTUser
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public Task<IEnumerable<string>> GetUserRolesAsync(BTUser user);
        public Task<List<BTUser>> GetUsersInRoleAsync(string roleName, int companyId);

        public Task<bool> IsUserInRoleAsync(BTUser member, string roleName);
        /// <summary>
        /// Remove provided BTUser from a single role
        /// </summary>
        /// <param name="user"></param>
        /// <param name="roleName"></param>
        /// <returns></returns>
        public Task<bool> RemoveUserFromRoleAsync(BTUser user, string roleName);

        /// <summary>
        /// Remove provided BTUser from the list of roles
        /// </summary>
        /// <param name="user"></param>
        /// <param name="roleName"></param>
        /// <returns></returns>
        public Task<bool> RemoveUserFromRolesAsync(BTUser user, IEnumerable<string> roleNames);
    }
}
