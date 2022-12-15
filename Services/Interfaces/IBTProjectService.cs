using BugTracker.Models;

namespace BugTracker.Services.Interfaces
{
    public interface IBTProjectService
    {
        public Task<List<Project>> GetAllProjectsByCompanyIdAsync(int companyId);
        public Task<List<Project>> GetArchivedProjectsByCompanyIdAsync(int companyId);
        public Task AddProjectAsync(Project project);
        public Task<bool> AddProjectManagerAsync(string userId, int projectId);
        public Task<bool> AddMemberToProjectAsync(BTUser member, int projectId);
        public Task<Project> GetProjectByIdAsync(int projectId, int companyId);
        public Task<BTUser> GetProjectManagerAsync(int projectId);
        public Task<IEnumerable<ProjectPriority>> GetProjectPrioritiesAsync();
        public Task<List<Project>?> GetAllProjectsByPriorityAsync(int companyId, int priority, string priorityName);
        public Task RemoveProjectManagerAsync(int projectId);
        public Task UpdateProjectAsync(Project project);
        public Task ArchiveProjectAsync(int projectId, Project project);
        public Task RestoreProjectAsync(int projectId, Project project);
        public Task<bool> RemoveMemberFromProjectAsync(BTUser member, int projectId, int companyId);
        public Task<int> LookUpProjectPriorityId(string priorityName);
        public Task<List<Project>?> GetUserProjectsAsync(string userId);
        public Task<bool> IsAssignedProjectManagerAsync(string userId, int projectId);
		public Task<List<BTUser>> GetAllProjectMembersExceptPMAsync(int projectId, BTUser member);
		public Task<List<BTUser>> GetProjectMembersByRoleAsync(int projectId, string role);
		public Task<List<Project>> GetUnassignedProjectsAsync(int companyId);

	}
}
