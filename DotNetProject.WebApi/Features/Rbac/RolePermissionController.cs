using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Common;
using DotNetProject.Domain.Features.Rbac;
using DotNetProject.WebApi.Features.Shared;
using Microsoft.AspNetCore.Mvc;

namespace DotNetProject.WebApi.Features.Rbac
{
    [ApiController]
    [Route("api/rbac")]
    [Route("api/role-permission")]
    public class RolePermissionController : BaseController
    {
        private readonly IRolePermissionService _rolePermissionService;

        public RolePermissionController(IRolePermissionService rolePermissionService)
        {
            _rolePermissionService = rolePermissionService;
        }

        [HttpGet("roles")]
        [HttpPost("roles")]
        public async Task<IActionResult> GetRoles(CancellationToken cancellationToken)
        {
            var result = await _rolePermissionService.GetRolesAsync(cancellationToken);
            return ToActionResult(result);
        }

        [HttpGet("permissions")]
        [HttpPost("permissions")]
        public async Task<IActionResult> GetPermissions(CancellationToken cancellationToken)
        {
            var result = await _rolePermissionService.GetAllPermissionsAsync(cancellationToken);
            return ToActionResult(result);
        }

        [HttpPost("role")]
        [HttpPost("role/save")]
        [HttpPost("roles/create")]
        public async Task<IActionResult> SaveRole([FromBody] SaveRoleRequest request, CancellationToken cancellationToken)
        {
            var result = await _rolePermissionService.SaveRoleAsync(request, cancellationToken);
            return ToActionResult(result);
        }

        [HttpDelete("role/{id:int}")]
        public async Task<IActionResult> DeleteRole(int id, CancellationToken cancellationToken)
        {
            var result = await _rolePermissionService.DeleteRoleAsync(id, cancellationToken);
            return ToActionResult(result);
        }

        [HttpPost("roles/delete")]
        public async Task<IActionResult> DeleteRolePost([FromBody] int id, CancellationToken cancellationToken)
        {
            var result = await _rolePermissionService.DeleteRoleAsync(id, cancellationToken);
            return ToActionResult(result);
        }

        [HttpGet("user/{userId:int}/roles")]
        public async Task<IActionResult> GetUserRoles(int userId, CancellationToken cancellationToken)
        {
            var result = await _rolePermissionService.GetUserRoleNamesAsync(userId, cancellationToken);
            return ToActionResult(result);
        }

        [HttpGet("user/{userId:int}/role-ids")]
        public async Task<IActionResult> GetUserRoleIds(int userId, CancellationToken cancellationToken)
        {
            var result = await _rolePermissionService.GetUserRoleIdsAsync(userId, cancellationToken);
            return ToActionResult(result);
        }

        [HttpGet("user/{userId:int}/permissions")]
        public async Task<IActionResult> GetUserPermissions(int userId, CancellationToken cancellationToken)
        {
            var result = await _rolePermissionService.GetUserPermissionsAsync(userId, cancellationToken);
            return ToActionResult(result);
        }

        [HttpPost("user/assign-roles")]
        public async Task<IActionResult> AssignUserRoles([FromBody] AssignUserRolesRequest request, CancellationToken cancellationToken)
        {
            var result = await _rolePermissionService.AssignUserRolesAsync(request, cancellationToken);
            return ToActionResult(result);
        }

        [HttpGet("user-assignments")]
        public async Task<IActionResult> GetUserAssignments(CancellationToken cancellationToken)
        {
            var result = await _rolePermissionService.GetUserAssignmentsAsync(cancellationToken);
            return ToActionResult(result);
        }

        [HttpPost("user/{userId:int}/toggle-status")]
        public async Task<IActionResult> ToggleUserStatus(int userId, CancellationToken cancellationToken)
        {
            var result = await _rolePermissionService.ToggleUserStatusAsync(userId, cancellationToken);
            return ToActionResult(result);
        }
    }
}
