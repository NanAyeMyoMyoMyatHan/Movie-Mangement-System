using System;
using System.Linq;
using System.Threading.Tasks;
using DotNetProject.Database.AppDbModels;
using DotNetProject.Domain.Features.Audit;
using Microsoft.EntityFrameworkCore;

namespace DotNetProject.WebApi.Features.Audit;

public class AuditServices : IAuditInterface
{
    private readonly AppDbContext _db;

    public AuditServices(AppDbContext db)
    {
        _db = db;
    }

    public async Task LogActionAsync(AuditLogCreateRequestModel model)
    {
        try
        {
            var log = new AuditLog
            {
                UserId = model.UserId,
                UserName = string.IsNullOrWhiteSpace(model.UserName) ? "System" : model.UserName.Trim(),
                Action = model.Action.Trim(),
                TargetEntity = model.TargetEntity.Trim(),
                TargetId = model.TargetId?.Trim(),
                Details = model.Details,
                IpAddress = model.IpAddress,
                Timestamp = DateTime.UtcNow
            };

            _db.AuditLogs.Add(log);
            await _db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AuditServices] Error logging audit action: {ex.Message}");
        }
    }

    public async Task<PagedAuditLogResult> GetAuditLogsAsync(AuditLogFilterRequest filter)
    {
        var query = _db.AuditLogs.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Action))
        {
            query = query.Where(a => a.Action == filter.Action.Trim());
        }

        if (!string.IsNullOrWhiteSpace(filter.UserName))
        {
            query = query.Where(a => a.UserName.Contains(filter.UserName.Trim()));
        }

        if (!string.IsNullOrWhiteSpace(filter.TargetEntity))
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
