using System.Threading.Tasks;

namespace DotNetProject.Domain.Features.Audit;

public interface IAuditInterface
{
    Task LogActionAsync(AuditLogCreateRequestModel model);
    Task<PagedAuditLogResult> GetAuditLogsAsync(AuditLogFilterRequest filter);
}
