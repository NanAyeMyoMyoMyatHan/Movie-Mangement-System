using DotNetProject.Balzor.Components.Shared;
using DotNetProject.Domain.Features.Rbac;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetProject.Balzor.Components.Pages.Admin
{
    public partial class AdminRoles
    {
        [Inject]
        public HttpClientService ApiClient { get; set; } = default!;

        [Inject]
        public AuthService AuthService { get; set; } = default!;

        [Inject]
        public NavigationManager Nav { get; set; } = default!;

        [Inject]
        public AuditLogService AuditLog { get; set; } = default!;

        protected List<RoleDto> RolesList { get; set; } = new();
        protected List<PermissionDto> AllPermissions { get; set; } = new();
        protected bool IsLoading { get; set; } = true;

        protected string? SuccessMessage { get; set; }
        protected string? ErrorMessage { get; set; }

        // Modal State
        protected bool ShowEditModal { get; set; } = false;
        protected bool IsSaving { get; set; } = false;
        protected string ModalTitle { get; set; } = "Create New Role";

        protected SaveRoleRequest CurrentRoleRequest { get; set; } = new();
        protected HashSet<int> SelectedPermissionIds { get; set; } = new();

        protected IEnumerable<IGrouping<string, PermissionDto>> GroupedPermissions =>
            AllPermissions.GroupBy(p => p.Module);

        protected override async Task OnInitializedAsync()
        {
            if (AuthService.IsAuthenticated() && AuthService.HasPermission("Role.View"))
            {
                await LoadDataAsync();
            }
        }

        protected override void OnAfterRender(bool firstRender)
        {
            if (firstRender && (!AuthService.IsAuthenticated() || !AuthService.HasPermission("Role.View")))
            {
                Nav.NavigateTo("/login?returnUrl=/admin/roles");
            }
        }

        protected async Task LoadDataAsync()
        {
            IsLoading = true;
            ErrorMessage = null;

            try
            {
                var rolesRes = await ApiClient.GetRolesAsync();
                RolesList = rolesRes?.Data ?? new();

                var permsRes = await ApiClient.GetAllPermissionsAsync();
                AllPermissions = permsRes?.Data ?? new();
            }
            catch (Exception ex)
            {
                ErrorMessage = "Failed to load roles: " + ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }

        protected void OpenCreateModal()
        {
            CurrentRoleRequest = new SaveRoleRequest
            {
                RoleId = null,
                RoleName = "",
                Description = "",
                IsActive = true,
                PermissionIds = new()
            };
            SelectedPermissionIds = new();
            ModalTitle = "Create New Role";
            ErrorMessage = null;
            SuccessMessage = null;
            ShowEditModal = true;
        }

        protected void OpenEditModal(RoleDto role)
        {
            CurrentRoleRequest = new SaveRoleRequest
            {
                RoleId = role.RoleId,
                RoleName = role.RoleName,
                Description = role.Description,
                IsActive = role.IsActive,
                PermissionIds = new List<int>(role.PermissionIds)
            };
            SelectedPermissionIds = new HashSet<int>(role.PermissionIds);
            ModalTitle = role.IsSystemRole ? $"Edit System Role: {role.RoleName}" : $"Edit Role: {role.RoleName}";
            ErrorMessage = null;
            SuccessMessage = null;
            ShowEditModal = true;
        }

        protected void CloseModal()
        {
            ShowEditModal = false;
        }

        protected void TogglePermission(int permissionId)
        {
            if (SelectedPermissionIds.Contains(permissionId))
                SelectedPermissionIds.Remove(permissionId);
            else
                SelectedPermissionIds.Add(permissionId);
        }

        protected void ToggleModuleAll(string module, bool selectAll)
        {
            var modulePermIds = AllPermissions
                .Where(p => p.Module.Equals(module, StringComparison.OrdinalIgnoreCase))
                .Select(p => p.PermissionId);

            foreach (var id in modulePermIds)
            {
                if (selectAll)
                    SelectedPermissionIds.Add(id);
                else
                    SelectedPermissionIds.Remove(id);
            }
        }

        protected async Task SaveRoleAsync()
        {
            if (string.IsNullOrWhiteSpace(CurrentRoleRequest.RoleName))
            {
                ErrorMessage = "Role name is required.";
                return;
            }

            IsSaving = true;
            ErrorMessage = null;

            try
            {
                CurrentRoleRequest.PermissionIds = SelectedPermissionIds.ToList();
                var result = await ApiClient.SaveRoleAsync(CurrentRoleRequest);

                if (result != null && result.IsSuccess)
                {
                    SuccessMessage = result.Message;
                    var action = CurrentRoleRequest.RoleId.HasValue ? "UPDATE_ROLE" : "CREATE_ROLE";
                    await AuditLog.LogActionAsync(action, "Role", CurrentRoleRequest.RoleId?.ToString() ?? CurrentRoleRequest.RoleName, $"{action} '{CurrentRoleRequest.RoleName}' with {SelectedPermissionIds.Count} permissions.");
                    ShowEditModal = false;
                    await LoadDataAsync();
                    await AuthService.RefreshPermissionsAsync();
                }
                else
                {
                    ErrorMessage = result?.Message ?? "Failed to save role.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Error saving role: " + ex.Message;
            }
            finally
            {
                IsSaving = false;
            }
        }

        protected async Task DeleteRoleAsync(RoleDto role)
        {
            if (role.IsSystemRole)
            {
                ErrorMessage = "System roles cannot be deleted.";
                return;
            }

            ErrorMessage = null;
            SuccessMessage = null;

            try
            {
                var result = await ApiClient.DeleteRoleAsync(role.RoleId);
                if (result != null && result.IsSuccess)
                {
                    SuccessMessage = result.Message;
                    await AuditLog.LogActionAsync("DELETE_ROLE", "Role", role.RoleId.ToString(), $"Deleted role '{role.RoleName}' (ID: {role.RoleId})");
                    await LoadDataAsync();
                    await AuthService.RefreshPermissionsAsync();
                }
                else
                {
                    ErrorMessage = result?.Message ?? "Failed to delete role.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Error deleting role: " + ex.Message;
            }
        }
    }
}
