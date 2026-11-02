using Common;
using DotNetProject.Domain.Features.Contents;
using DotNetProject.Domain.Features.Movies;
using DotNetProject.Domain.Features.Thai;
using Newtonsoft.Json;
using System.Text;

namespace DotNetProject.Balzor.Components.Shared
{
    public class HttpClientService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public HttpClientService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // --- Generic helpers ---

        public async Task<T?> PostAsync<T>(string url, object request)
        {
            try
            {
                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var client = _httpClientFactory.CreateClient("Api");
                var response = await client.PostAsync(url, content);

                var jsonStr = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(jsonStr);
            }
            catch
            {
                return default;
            }
        }

        public async Task<T?> ExecuteAsync<T>(string url, EnumHttpMethod method, object? obj = null)
        {
            HttpResponseMessage? responseMessage = null;
            HttpContent? content = null;

            if (obj != null)
            {
                string jsonString = JsonConvert.SerializeObject(obj);
                content = new StringContent(jsonString, Encoding.UTF8, "application/json");
            }

            var client = _httpClientFactory.CreateClient("Api");
            switch (method)
            {
                case EnumHttpMethod.Get:
                    responseMessage = await client.GetAsync(url);
                    break;
                case EnumHttpMethod.Post:
                    responseMessage = await client.PostAsync(url, content);
                    break;
                case EnumHttpMethod.Put:
                    responseMessage = await client.PutAsync(url, content);
                    break;
                case EnumHttpMethod.Patch:
                    responseMessage = await client.PatchAsync(url, content);
                    break;
                case EnumHttpMethod.Delete:
                    responseMessage = await client.DeleteAsync(url);
                    break;
            }

            if (responseMessage == null) return default;

            var result = await responseMessage.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(result);
        }

        // --- Movie API client methods 

        public Task<Result<GetMoviePagedListResponse>?> GetMoviesPagedAsync(GetMoviePagedListRequest request) =>
            PostAsync<Result<GetMoviePagedListResponse>>("api/movie/paged-list", request);

        public Task<Result<GetMovieDetailResponse>?> GetMovieDetailAsync(int id) =>
            PostAsync<Result<GetMovieDetailResponse>>("api/movie/detail", new GetMovieDetailRequest { Id = id });

        public Task<Result<CreateMovieResponse>?> CreateMovieAsync(CreateMovieRequest request) =>
            PostAsync<Result<CreateMovieResponse>>("api/movie/create", request);

        public Task<Result<UpdateMovieResponse>?> UpdateMovieAsync(UpdateMovieRequest request) =>
            PostAsync<Result<UpdateMovieResponse>>("api/movie/update", request);

        public Task<Result<DeleteMovieResponse>?> DeleteMovieAsync(int id) =>
            PostAsync<Result<DeleteMovieResponse>>("api/movie/delete", new DeleteMovieRequest { Id = id });

        // --- Content API client methods

        public Task<Result<GetContentPagedListResponse>?> GetContentsPagedAsync(GetContentPagedListRequest request) =>
            PostAsync<Result<GetContentPagedListResponse>>("api/contents/paged-list", request);

        public Task<Result<ContentDetailDto>?> GetContentWithEpisodesAsync(long id) =>
            ExecuteAsync<Result<ContentDetailDto>>($"api/contents/{id}", EnumHttpMethod.Get);

        public Task<Result<GetContentDetailResponse>?> GetContentDetailAsync(long id) =>
            PostAsync<Result<GetContentDetailResponse>>("api/contents/detail", new GetContentDetailRequest { ContentId = id });



        public Task<Result<CreateContentResponse>?> CreateContentAsync(CreateContentRequest request) =>
            PostAsync<Result<CreateContentResponse>>("api/contents/create", request);

        public Task<Result<UpdateContentResponse>?> UpdateContentAsync(UpdateContentRequest request) =>
            PostAsync<Result<UpdateContentResponse>>("api/contents/update", request);

        public Task<Result<DeleteContentResponse>?> DeleteContentAsync(long id) =>
            PostAsync<Result<DeleteContentResponse>>("api/contents/delete", new DeleteContentRequest { ContentId = id });

        public Task<Result<GetThaiPagedListResponse>?> GetThaiPagedAsync(GetThaiPagedListRequest request) =>
            PostAsync<Result<GetThaiPagedListResponse>>("api/thai/paged-list", request);
    }

    public enum EnumHttpMethod { None, Get, Post, Put, Patch, Delete }
}
