using System;
using System.Collections.Generic;

namespace DotNetProject.Domain.Features.Audit;

public class AuditLogDto
{
    public long Id { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string TargetEntity { get; set; } = string.Empty;
    public string? TargetId { get; set; }
    public string? Details { get; set; }
    public DateTime Timestamp { get; set; }
    public string? IpAddress { get; set; }
}

public class AuditLogFilterRequest
{
    public string? Action { get; set; }
    public string? UserName { get; set; }
    public string? TargetEntity { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class AuditLogCreateRequestModel
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string TargetEntity { get; set; } = string.Empty;
    public string? TargetId { get; set; }
    public string? Details { get; set; }
    public string? IpAddress { get; set; }
}

public class PagedAuditLogResult
{
    public List<AuditLogDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / (PageSize > 0 ? PageSize : 1));
}
