using GarageControl.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GarageControl.Core.Contracts
{
    public interface IAdminService
    {
        Task<List<UserAdminVM>> GetUsersAsync();
        Task<MethodResponse> ToggleUserBlockAsync(string userId);
    }
}
