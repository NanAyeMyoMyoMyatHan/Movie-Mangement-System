using System;

namespace DotNetProject.Database.AppDbModels;

public partial class AuditLog
{
    public long Id { get; set; }

    public int UserId { get; set; }

    public string UserName { get; set; } = string.Empty;

    public string Action { get; set; } = string.Empty;

    public string TargetEntity { get; set; } = string.Empty;

    public string? TargetId { get; set; }

    public string? Details { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public string? IpAddress { get; set; }
}
