using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetProject.Domain.Features.Thai
{
    public interface IThaiInterface
    {
        Task<Result<GetThaiPagedListResponse>> GetPagedListAsync(
            GetThaiPagedListRequest request, CancellationToken cancellationToken = default);

        Task<Result<ThaiDetailResponse>> GetDetailAsync(
            GetThaiDetailRequest request, CancellationToken cancellationToken = default); 

        Task<Result<CreateThaiResponse>> CreateAsync(
            CreateThaiReques request, CancellationToken cancellationToken = default);

        Task<Result<UpdateThaiResponse>> UpdateAsync(
            UpdateThaiRequest request, CancellationToken cancellationToken);
        
        Task<Result<DeleteThaiResponse>> DeleteAsync(
            DeleteThaiRequest request, CancellationToken cancellationToken = default);
    }

}
