using Common;
using DotNetProject.Domain.Features.Audit;
using DotNetProject.Domain.Features.Contents;
using DotNetProject.Domain.Features.Favorites;
using DotNetProject.Domain.Features.Movies;
using DotNetProject.Domain.Features.Rbac;
using DotNetProject.Domain.Features.Thai;
using DotNetProject.Domain.Features.Tmdb;
using DotNetProject.Domain.Features.Users;
using DotNetProject.Domain.Features.WatchHistory;
using DotNetProject.Domain.Features.WatchLater;
using Newtonsoft.Json;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text;

namespace DotNetProject.Balzor.Components.Shared
{
    /// <summary>
    /// In-memory payload for an image/media file chosen by the user via InputFile,
    /// ready to stream reliably over multipart/form-data.
    /// </summary>
    public sealed record UploadedFilePayload(
        string FileName,
        string ContentType,
        byte[] Data,
        string PreviewDataUrl);

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
            try
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
            catch
            {
                return default;
            }
        }

        // --- User Authentication & Profile API client methods ---

        public Task<UserLoginResponseModel?> LoginUserAsync(UserLoginRequestModel request) =>
            PostAsync<UserLoginResponseModel>("api/User/login", request);

        public Task<UserCreateResponseModel?> RegisterUserAsync(UserCreateRequestModel request) =>
            PostAsync<UserCreateResponseModel>("api/User", request);

        public Task<UserProfileDto?> GetUserProfileAsync(int userId) =>
            ExecuteAsync<UserProfileDto>($"api/User/profile/{userId}", EnumHttpMethod.Get);

        public Task<UserProfileUpdateResponseModel?> UpdateUserProfileAsync(UserProfileUpdateRequestModel request) =>
            ExecuteAsync<UserProfileUpdateResponseModel>("api/User/profile", EnumHttpMethod.Put, request);

        public Task<ChangePasswordResponseModel?> ChangeUserPasswordAsync(ChangePasswordRequestModel request) =>
            PostAsync<ChangePasswordResponseModel>("api/User/change-password", request);

        public async Task<string?> UploadAvatarAsync(int userId, Stream fileStream, string fileName)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("Api");
                using var content = new MultipartFormDataContent();
                var streamContent = new StreamContent(fileStream);
                content.Add(streamContent, "avatarFile", fileName);

                var response = await client.PostAsync($"api/User/profile/avatar?userId={userId}", content);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                    if (dict != null && dict.TryGetValue("avatarUrl", out var urlObj) && urlObj != null)
                    {
                        return urlObj.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[HttpClientService] UploadAvatarAsync error: {ex.Message}");
            }
            return null;
        }

        // --- Movie API client methods 

        public Task<Result<GetMoviePagedListResponse>?> GetMoviesPagedAsync(GetMoviePagedListRequest request) =>
            PostAsync<Result<GetMoviePagedListResponse>>("api/movie/paged-list", request);

        public Task<Result<GetMovieDetailResponse>?> GetMovieDetailAsync(int id) =>
            PostAsync<Result<GetMovieDetailResponse>>("api/movie/detail", new GetMovieDetailRequest { Id = id });

        public async Task<Result<CreateMovieResponse>?> CreateMovieAsync(CreateMovieRequest request, UploadedFilePayload? posterFile = null)
        {
            using var form = BuildMovieForm(request.Title, request.Genre, request.ReleaseYear, request.Description, request.PosterUrl, request.TmdbId, request.IsManual, posterFile);
            return await PostMultipartAsync<Result<CreateMovieResponse>>("api/movie/create", form);
        }

        public async Task<Result<UpdateMovieResponse>?> UpdateMovieAsync(UpdateMovieRequest request, UploadedFilePayload? posterFile = null)
        {
            using var form = BuildMovieForm(request.Title, request.Genre, request.ReleaseYear, request.Description, request.PosterUrl, request.TmdbId, request.IsManual, posterFile);
            AddFormField(form, "Id", request.Id.ToString(CultureInfo.InvariantCulture));
            return await PostMultipartAsync<Result<UpdateMovieResponse>>("api/movie/update", form);
        }

        public Task<Result<DeleteMovieResponse>?> DeleteMovieAsync(int id) =>
            PostAsync<Result<DeleteMovieResponse>>("api/movie/delete", new DeleteMovieRequest { Id = id });

        // --- Content API client methods

        public Task<Result<GetContentPagedListResponse>?> GetContentsPagedAsync(GetContentPagedListRequest request) =>
            PostAsync<Result<GetContentPagedListResponse>>("api/contents/paged-list", request);

        public Task<Result<ContentDetailDto>?> GetContentWithEpisodesAsync(long id) =>
            ExecuteAsync<Result<ContentDetailDto>>($"api/contents/{id}", EnumHttpMethod.Get);

        public Task<Result<GetContentDetailResponse>?> GetContentDetailAsync(long id) =>
            PostAsync<Result<GetContentDetailResponse>>("api/contents/detail", new GetContentDetailRequest { ContentId = id });

        public async Task<Result<CreateContentResponse>?> CreateContentAsync(
            CreateContentRequest request,
            UploadedFilePayload? posterFile = null,
            UploadedFilePayload? bannerFile = null)
        {
            using var form = BuildContentForm(request.ContentTypeId, request.Title, request.Description, request.PosterUrl, request.BannerUrl, request.ReleaseDate, request.Country, request.Language, request.Status, posterFile, bannerFile);
            return await PostMultipartAsync<Result<CreateContentResponse>>("api/contents/create", form);
        }

        public async Task<Result<UpdateContentResponse>?> UpdateContentAsync(
            UpdateContentRequest request,
            UploadedFilePayload? posterFile = null,
            UploadedFilePayload? bannerFile = null)
        {
            using var form = BuildContentForm(request.ContentTypeId, request.Title, request.Description, request.PosterUrl, request.BannerUrl, request.ReleaseDate, request.Country, request.Language, request.Status, posterFile, bannerFile);
            AddFormField(form, "ContentId", request.ContentId.ToString(CultureInfo.InvariantCulture));
            return await PostMultipartAsync<Result<UpdateContentResponse>>("api/contents/update", form);
        }

        public Task<Result<DeleteContentResponse>?> DeleteContentAsync(long id) =>
            PostAsync<Result<DeleteContentResponse>>("api/contents/delete", new DeleteContentRequest { ContentId = id });

        public Task<Result<GetThaiPagedListResponse>?> GetThaiPagedAsync(GetThaiPagedListRequest request) =>
            PostAsync<Result<GetThaiPagedListResponse>>("api/Thai/GetPagedList", request);

        private async Task<T?> PostMultipartAsync<T>(string url, MultipartFormDataContent form)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("Api");
                var response = await client.PostAsync(url, form);
                var jsonStr = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(jsonStr);
            }
            catch
            {
                return default;
            }
        }

        private static MultipartFormDataContent BuildMovieForm(
            string? title, string? genre, int? releaseYear, string? description,
            string? posterUrl, int? tmdbId, bool isManual, UploadedFilePayload? posterFile)
        {
            var form = new MultipartFormDataContent();
            AddFormField(form, "Title", title);
            AddFormField(form, "Genre", genre);
            if (releaseYear.HasValue) AddFormField(form, "ReleaseYear", releaseYear.Value.ToString(CultureInfo.InvariantCulture));
            AddFormField(form, "Description", description);
            AddFormField(form, "PosterUrl", posterUrl);
            if (tmdbId.HasValue) AddFormField(form, "TmdbId", tmdbId.Value.ToString(CultureInfo.InvariantCulture));
            AddFormField(form, "IsManual", isManual ? "true" : "false");
            AddFormFile(form, "PosterFile", posterFile);
            return form;
        }

        private static MultipartFormDataContent BuildContentForm(
            int contentTypeId, string? title, string? description, string? posterUrl, string? bannerUrl,
            DateOnly? releaseDate, string? country, string? language, string status,
            UploadedFilePayload? posterFile, UploadedFilePayload? bannerFile)
        {
            var form = new MultipartFormDataContent();
            AddFormField(form, "ContentTypeId", contentTypeId.ToString(CultureInfo.InvariantCulture));
            AddFormField(form, "Title", title);
            AddFormField(form, "Description", description);
            AddFormField(form, "PosterUrl", posterUrl);
            AddFormField(form, "BannerUrl", bannerUrl);
            if (releaseDate.HasValue) AddFormField(form, "ReleaseDate", releaseDate.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
            AddFormField(form, "Country", country);
            AddFormField(form, "Language", language);
            AddFormField(form, "Status", status);
            AddFormFile(form, "PosterFile", posterFile);
            AddFormFile(form, "BannerFile", bannerFile);
            return form;
        }

        private static void AddFormField(MultipartFormDataContent form, string name, string? value)
        {
            if (value != null)
                form.Add(new StringContent(value, Encoding.UTF8), name);
        }

        private static void AddFormFile(MultipartFormDataContent form, string name, UploadedFilePayload? file)
        {
            if (file == null || file.Data.Length == 0) return;
            var content = new ByteArrayContent(file.Data);
            content.Headers.ContentType = MediaTypeHeaderValue.TryParse(file.ContentType, out var mediaType)
                ? mediaType
                : new MediaTypeHeaderValue("application/octet-stream");
            form.Add(content, name, string.IsNullOrWhiteSpace(file.FileName) ? "upload.bin" : file.FileName);
        }

        // --- RBAC API client methods ---

        public Task<Result<List<RoleDto>>?> GetRolesAsync() =>
            ExecuteAsync<Result<List<RoleDto>>>("api/rbac/roles", EnumHttpMethod.Get);

        public Task<Result<List<PermissionDto>>?> GetAllPermissionsAsync() =>
            ExecuteAsync<Result<List<PermissionDto>>>("api/rbac/permissions", EnumHttpMethod.Get);

        public Task<Result<RoleDto>?> SaveRoleAsync(SaveRoleRequest request) =>
            PostAsync<Result<RoleDto>>("api/rbac/role", request);

        public Task<Result<bool>?> DeleteRoleAsync(int roleId) =>
            ExecuteAsync<Result<bool>>($"api/rbac/role/{roleId}", EnumHttpMethod.Delete);

        public Task<Result<List<int>>?> GetUserRoleIdsAsync(int userId) =>
            ExecuteAsync<Result<List<int>>>($"api/rbac/user/{userId}/role-ids", EnumHttpMethod.Get);

        public Task<Result<List<string>>?> GetUserRoleNamesAsync(int userId) =>
            ExecuteAsync<Result<List<string>>>($"api/rbac/user/{userId}/roles", EnumHttpMethod.Get);

        public Task<Result<List<string>>?> GetUserPermissionsAsync(int userId) =>
            ExecuteAsync<Result<List<string>>>($"api/rbac/user/{userId}/permissions", EnumHttpMethod.Get);

        public Task<Result<bool>?> AssignUserRolesAsync(AssignUserRolesRequest request) =>
            PostAsync<Result<bool>>("api/rbac/user/assign-roles", request);

        public Task<Result<List<UserRoleAssignmentDto>>?> GetUserAssignmentsAsync() =>
            ExecuteAsync<Result<List<UserRoleAssignmentDto>>>("api/rbac/user-assignments", EnumHttpMethod.Get);

        public Task<Result<bool>?> ToggleUserStatusAsync(int userId) =>
            PostAsync<Result<bool>>($"api/rbac/user/{userId}/toggle-status", new { });

        public Task<List<UserModel>?> GetAllUsersAsync() =>
            ExecuteAsync<List<UserModel>>("api/user", EnumHttpMethod.Get);

        // --- Audit Log API client methods ---
        public Task<PagedAuditLogResult?> GetAuditLogsAsync(AuditLogFilterRequest filter)
        {
            var qs = new List<string>
            {
                $"pageNumber={filter.PageNumber}",
                $"pageSize={filter.PageSize}"
            };
            if (!string.IsNullOrWhiteSpace(filter.Action)) qs.Add($"action={Uri.EscapeDataString(filter.Action)}");
            if (!string.IsNullOrWhiteSpace(filter.UserName)) qs.Add($"userName={Uri.EscapeDataString(filter.UserName)}");
            if (!string.IsNullOrWhiteSpace(filter.TargetEntity)) qs.Add($"targetEntity={Uri.EscapeDataString(filter.TargetEntity)}");
            if (filter.FromDate.HasValue) qs.Add($"fromDate={filter.FromDate.Value:O}");
            if (filter.ToDate.HasValue) qs.Add($"toDate={filter.ToDate.Value:O}");

            return ExecuteAsync<PagedAuditLogResult>($"api/audit?{string.Join("&", qs)}", EnumHttpMethod.Get);
        }

        public Task<object?> LogAuditActionAsync(AuditLogCreateRequestModel model) =>
            PostAsync<object>("api/audit", model);

        // --- TMDB API client methods ---
        public Task<TmdbLookupResponse<List<TmdbSearchResultDto>>?> SearchTmdbMoviesAsync(string query, int maxResults = 6) =>
            ExecuteAsync<TmdbLookupResponse<List<TmdbSearchResultDto>>>($"api/tmdb/search/movies?query={Uri.EscapeDataString(query)}&maxResults={maxResults}", EnumHttpMethod.Get);

        public Task<TmdbLookupResponse<List<TmdbSearchResultDto>>?> SearchTmdbDramasAsync(string query, int maxResults = 6) =>
            ExecuteAsync<TmdbLookupResponse<List<TmdbSearchResultDto>>>($"api/tmdb/search/dramas?query={Uri.EscapeDataString(query)}&maxResults={maxResults}", EnumHttpMethod.Get);

        public Task<TmdbLookupResponse<TmdbMovieDetailsDto>?> GetTmdbMovieAsync(int tmdbId) =>
            ExecuteAsync<TmdbLookupResponse<TmdbMovieDetailsDto>>($"api/tmdb/movies/{tmdbId}", EnumHttpMethod.Get);

        public Task<TmdbLookupResponse<TmdbMovieDetailsDto>?> GetTmdbDramaAsync(int tmdbId) =>
            ExecuteAsync<TmdbLookupResponse<TmdbMovieDetailsDto>>($"api/tmdb/dramas/{tmdbId}", EnumHttpMethod.Get);

        public Task<TmdbImportResult?> ImportTmdbMovieAsync(int tmdbId, string? customCategory = null) =>
            PostAsync<TmdbImportResult>($"api/tmdb/import/movie/{tmdbId}?customCategory={Uri.EscapeDataString(customCategory ?? string.Empty)}", new { });

        public Task<TmdbImportResult?> ImportTmdbDramaAsync(int tmdbId, string? customCategory = null) =>
            PostAsync<TmdbImportResult>($"api/tmdb/import/drama/{tmdbId}?customCategory={Uri.EscapeDataString(customCategory ?? string.Empty)}", new { });

        // --- Favorites API client methods ---
        public Task<List<FavoriteItem>?> GetFavoritesAsync(int userId) =>
            ExecuteAsync<List<FavoriteItem>>($"api/favorites/user/{userId}", EnumHttpMethod.Get);

        public Task<object?> AddFavoriteAsync(int userId, FavoriteItem item) =>
            PostAsync<object>($"api/favorites/user/{userId}", item);

        public Task<object?> RemoveFavoriteAsync(int userId, long contentId, bool isContent = true) =>
            ExecuteAsync<object>($"api/favorites/user/{userId}?contentId={contentId}&isContent={isContent}", EnumHttpMethod.Delete);

        public async Task<bool> IsFavoriteAsync(int userId, long contentId, bool isContent = true)
        {
            var res = await ExecuteAsync<Dictionary<string, bool>>($"api/favorites/check?userId={userId}&contentId={contentId}&isContent={isContent}", EnumHttpMethod.Get);
            return res != null && res.TryGetValue("isFavorite", out var isFav) && isFav;
        }

        // --- Watch Later API client methods ---
        public Task<List<WatchLaterItem>?> GetWatchLaterAsync(int userId) =>
            ExecuteAsync<List<WatchLaterItem>>($"api/watchlater/user/{userId}", EnumHttpMethod.Get);

        public Task<object?> AddWatchLaterAsync(int userId, WatchLaterItem item) =>
            PostAsync<object>($"api/watchlater/user/{userId}", item);

        public Task<object?> RemoveWatchLaterAsync(int userId, long contentId, bool isContent = true) =>
            ExecuteAsync<object>($"api/watchlater/user/{userId}?contentId={contentId}&isContent={isContent}", EnumHttpMethod.Delete);

        public async Task<bool> IsWatchLaterAsync(int userId, long contentId, bool isContent = true)
        {
            var res = await ExecuteAsync<Dictionary<string, bool>>($"api/watchlater/check?userId={userId}&contentId={contentId}&isContent={isContent}", EnumHttpMethod.Get);
            return res != null && res.TryGetValue("isWatchLater", out var isWl) && isWl;
        }

        // --- Watch History API client methods ---
        public Task<List<WatchHistoryItem>?> GetWatchHistoryAsync(int userId) =>
            ExecuteAsync<List<WatchHistoryItem>>($"api/watchhistory/user/{userId}", EnumHttpMethod.Get);

        public Task<WatchHistoryItem?> GetWatchHistoryItemAsync(int userId, long contentId, bool isContent = true, int episodeNumber = 1) =>
            ExecuteAsync<WatchHistoryItem>($"api/watchhistory/item?userId={userId}&contentId={contentId}&isContent={isContent}&episodeNumber={episodeNumber}", EnumHttpMethod.Get);

        public Task<object?> SaveWatchProgressAsync(int userId, WatchHistoryItem item) =>
            PostAsync<object>($"api/watchhistory/user/{userId}", item);

        public Task<object?> RemoveWatchHistoryAsync(int userId, long contentId, bool isContent = true) =>
            ExecuteAsync<object>($"api/watchhistory/user/{userId}?contentId={contentId}&isContent={isContent}", EnumHttpMethod.Delete);

        public Task<object?> ClearWatchHistoryAsync(int userId) =>
            ExecuteAsync<object>($"api/watchhistory/user/{userId}/clear", EnumHttpMethod.Delete);

        // --- Episode Quick URL API client method ---
        public Task<Result<bool>?> UpdateEpisodeQuickUrlAsync(UpdateEpisodeUrlRequest request) =>
            PostAsync<Result<bool>>("api/contents/episode/quick-url", request);
    }

    public enum EnumHttpMethod { None, Get, Post, Put, Patch, Delete }
}
