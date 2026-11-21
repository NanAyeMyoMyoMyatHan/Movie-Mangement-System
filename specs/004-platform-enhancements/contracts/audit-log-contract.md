# Contract: Audit Log & Activity Trail Protocol

**Feature Branch**: `004-platform-enhancements`  
**Date**: 2026-11-21  

This contract defines the data structures and API interfaces for recording and reviewing system audit logs.

---

## 1. Audit Log DTOs

```csharp
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
```

---

## 2. Service Interface (`IAuditLogService`)

```csharp
public interface IAuditLogService
{
    Task LogAsync(int userId, string userName, string action, string targetEntity, string? targetId, string? details, string? ipAddress = null);
    Task<PagedResult<AuditLogDto>> GetLogsAsync(AuditLogFilterRequest filter);
}
```

---

## 3. Standard Action Codes

| Action Code | Description | Example Target |
|---|---|---|
| `CREATE_CONTENT` | New drama/series created | Content ID `45` |
| `UPDATE_CONTENT` | Drama metadata updated | Content ID `45` |
| `DELETE_CONTENT` | Drama removed | Content ID `45` |
| `DELETE_MOVIE` | Movie removed | Movie ID `12` |
| `ASSIGN_ROLE` | Role granted to user | User ID `7`, Role `Admin` |
| `REVOKE_ROLE` | Role revoked from user | User ID `7`, Role `Moderator` |
| `UPDATE_PERMISSIONS` | Permissions granted/modified on role | Role ID `2` |
| `CHANGE_PASSWORD` | User changed password | User ID `3` |
