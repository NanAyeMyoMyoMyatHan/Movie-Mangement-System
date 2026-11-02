using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DotNetProject.Domain.Features.Thai;

using Common;
using DotNetProject.WebApi.Features.Shared;

namespace DotNetProject.WebApi.Features.Thai
{
    [Route("api/[controller]")]
    [ApiController]
    public class ThaiController : BaseController
    {
        private readonly IThaiInterface _thaiService;

        public ThaiController(IThaiInterface thaiService, IWebHostEnvironment environment = null)
        {
            _thaiService = thaiService;
            _environment = environment;
        }

        private readonly IWebHostEnvironment _environment;

        [HttpPost("GetPagedList")]
        public async Task<IActionResult> GetPagedList([FromBody] GetThaiPagedListRequest request,
            CancellationToken cancellationToken) =>
            ToActionResult(await _thaiService.GetPagedListAsync(request, cancellationToken));

    }
}
