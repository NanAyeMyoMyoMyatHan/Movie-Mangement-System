using System;
using System.Threading.Tasks;
using DotNetProject.Domain.Features.Audit;

namespace DotNetProject.Balzor.Components.Shared
{
    public class AuditLogService
    {
        private readonly HttpClientService _api;
        private readonly AuthService _authService;

        public AuditLogService(HttpClientService api, AuthService authService)
        {
            _api = api;
            _authService = authService;
        }

        public async Task LogActionAsync(string action, string targetEntity, string? targetId, string? details, string? ipAddress = null)
        {
            try
            {
                int userId = _authService.CurrentUser.Id > 0 ? _authService.CurrentUser.Id : 1;
                string userName = !string.IsNullOrWhiteSpace(_authService.CurrentUser.DisplayName) 
                    ? _authService.CurrentUser.DisplayName 
                    : (_authService.CurrentUser.UserName ?? "Admin");

                var model = new AuditLogCreateRequestModel
                {
                    UserId = userId,
                    UserName = userName,
                    Action = action,
                    TargetEntity = targetEntity,
                    TargetId = targetId,
                    Details = details,
                    IpAddress = ipAddress
                };

                await _api.LogAuditActionAsync(model);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AuditLogService] LogActionAsync error: {ex.Message}");
            }
        }

        public async Task<PagedAuditLogResult> GetLogsAsync(AuditLogFilterRequest filter)
        {
            try
            {
                var result = await _api.GetAuditLogsAsync(filter);
                if (result != null)
                {
                    return result;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AuditLogService] GetLogsAsync error: {ex.Message}");
            }

            return new PagedAuditLogResult
            {
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize
            };
        }
    }
}
