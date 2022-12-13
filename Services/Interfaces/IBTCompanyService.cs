using BugTracker.Models;

namespace BugTracker.Services.Interfaces
{
	public interface IBTCompanyService
	{
		public Task<List<BTUser>> GetMembersAsync(int? companyId);
	}
}
