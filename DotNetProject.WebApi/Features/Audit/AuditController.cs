using System.Threading.Tasks;
using DotNetProject.Domain.Features.Audit;
using Microsoft.AspNetCore.Mvc;

namespace DotNetProject.WebApi.Features.Audit;

[Route("api/[controller]")]
[ApiController]
public class AuditController : ControllerBase
{
    private readonly IAuditInterface _auditService;

    public AuditController(IAuditInterface auditService)
    {
        _auditService = auditService;
    }

    [HttpPost]
    public async Task<IActionResult> LogAction([FromBody] AuditLogCreateRequestModel model)
    {
        await _auditService.LogActionAsync(model);
        return Ok(new { isSuccess = true, message = "Audit action recorded." });
    }

    [HttpGet]
    public async Task<IActionResult> GetAuditLogs([FromQuery] AuditLogFilterRequest filter)
    {
        var result = await _auditService.GetAuditLogsAsync(filter);
        return Ok(result);
    }
}
