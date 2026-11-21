using System;
using System.Linq;
using System.Threading.Tasks;
using DotNetProject.Database.AppDbModels;
using DotNetProject.Domain.Features.Audit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetProject.Balzor.Components.Shared
{
    public class AuditLogService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly AuthService _authService;

        public AuditLogService(IServiceProvider serviceProvider, AuthService authService)
        {
            _serviceProvider = serviceProvider;
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

                using var scope = _serviceProvider.CreateScope();
                var db = scope.ServiceProvider.GetService<AppDbContext>();
                if (db != null)
                {
                    db.AuditLogs.Add(new AuditLog
                    {
                        UserId = userId,
                        UserName = userName,
                        Action = action,
                        TargetEntity = targetEntity,
                        TargetId = targetId,
                        Details = details,
                        IpAddress = ipAddress,
                        Timestamp = DateTime.UtcNow
                    });
                    await db.SaveChangesAsync();
                }
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
                using var scope = _serviceProvider.CreateScope();
                var db = scope.ServiceProvider.GetService<AppDbContext>();
                if (db != null)
                {
                    var query = db.AuditLogs.AsNoTracking().AsQueryable();

                    if (!string.IsNullOrWhiteSpace(filter.Action) && filter.Action != "All")
                    {
                        query = query.Where(a => a.Action == filter.Action.Trim());
                    }

                    if (!string.IsNullOrWhiteSpace(filter.UserName))
                    {
                        var userTerm = filter.UserName.Trim().ToLower();
                        query = query.Where(a => a.UserName.ToLower().Contains(userTerm));
                    }

                    if (!string.IsNullOrWhiteSpace(filter.TargetEntity) && filter.TargetEntity != "All")
                    {
                        query = query.Where(a => a.TargetEntity == filter.TargetEntity.Trim());
                    }

                    if (filter.FromDate.HasValue)
                    {
                        query = query.Where(a => a.Timestamp >= filter.FromDate.Value);
                    }

                    if (filter.ToDate.HasValue)
                    {
                        query = query.Where(a => a.Timestamp <= filter.ToDate.Value);
                    }

                    int totalCount = await query.CountAsync();

                    var items = await query
                        .OrderByDescending(a => a.Timestamp)
                        .Skip((filter.PageNumber - 1) * filter.PageSize)
                        .Take(filter.PageSize)
                        .Select(a => new AuditLogDto
                        {
                            Id = a.Id,
                            UserId = a.UserId,
                            UserName = a.UserName,
                            Action = a.Action,
                            TargetEntity = a.TargetEntity,
                            TargetId = a.TargetId,
                            Details = a.Details,
                            Timestamp = a.Timestamp,
                            IpAddress = a.IpAddress
                        })
                        .ToListAsync();

                    return new PagedAuditLogResult
                    {
                        Items = items,
                        TotalCount = totalCount,
                        PageNumber = filter.PageNumber,
                        PageSize = filter.PageSize
                    };
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
