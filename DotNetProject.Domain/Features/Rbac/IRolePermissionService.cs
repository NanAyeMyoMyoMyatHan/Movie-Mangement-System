using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Common;

namespace DotNetProject.Domain.Features.Rbac
{
    public interface IRolePermissionService
    {
        Task<Result<List<RoleDto>>> GetRolesAsync(CancellationToken cancellationToken = default);
        Task<Result<List<PermissionDto>>> GetAllPermissionsAsync(CancellationToken cancellationToken = default);
        Task<Result<RoleDto>> SaveRoleAsync(SaveRoleRequest req, CancellationToken cancellationToken = default);
        Task<Result<bool>> DeleteRoleAsync(int roleId, CancellationToken cancellationToken = default);
        Task<Result<List<int>>> GetUserRoleIdsAsync(int userId, CancellationToken cancellationToken = default);
        Task<Result<List<string>>> GetUserRoleNamesAsync(int userId, CancellationToken cancellationToken = default);
        Task<Result<List<string>>> GetUserPermissionsAsync(int userId, CancellationToken cancellationToken = default);
        Task<Result<bool>> AssignUserRolesAsync(AssignUserRolesRequest req, CancellationToken cancellationToken = default);
        Task<Result<List<UserRoleAssignmentDto>>> GetUserAssignmentsAsync(CancellationToken cancellationToken = default);
    }
}
