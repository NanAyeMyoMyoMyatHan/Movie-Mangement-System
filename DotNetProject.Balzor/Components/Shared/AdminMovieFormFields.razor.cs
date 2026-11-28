using DotNetProject.Domain.Features.Tmdb;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace DotNetProject.Balzor.Components.Shared
{
    public partial class AdminMovieFormFields : IDisposable
    {
        [Inject] private TmdbService TmdbService { get; set; } = default!;

        [Parameter, EditorRequired] public MovieFormModel Model { get; set; } = default!;
        [Parameter] public string? TitleError { get; set; }
        [Parameter] public string SubmitLabel { get; set; } = "Save Movie";
        [Parameter] public bool Submitting { get; set; }
        [Parameter] public EventCallback OnSubmit { get; set; }
        [Parameter] public string? DeleteHref { get; set; }

        private bool _autoFillOnTitle = true;
        private bool _searchingTitle;
        private bool _fetchingById;
        private bool _disposed;
        private string? _tmdbStatus;
        private bool _tmdbStatusIsError;
        private string? _matchedOfficialTitle;
        private List<TmdbSearchResultDto> _suggestions = new();
        private CancellationTokenSource? _titleLookupCts;

        private async Task HandleTitleInputAsync(ChangeEventArgs e)
        {
            Model.Title = e.Value?.ToString();
            CancelPendingTitleLookup();

            if (!_autoFillOnTitle)
                return;

            var query = Model.Title?.Trim();
            if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
            {
                _searchingTitle = false;
                _tmdbStatus = null;
                _tmdbStatusIsError = false;
                _matchedOfficialTitle = null;
                _suggestions.Clear();
                return;
            }

            _titleLookupCts = new CancellationTokenSource();
            var token = _titleLookupCts.Token;

            try
            {
                await Task.Delay(600, token);
                await LookupByTitleCoreAsync(query, overwriteTitle: false, token);
            }
            catch (OperationCanceledException)
            {
                // Superseded by newer typing or manual action.
            }
        }

        private async Task HandleTitleKeyDownAsync(KeyboardEventArgs e)
        {
            if (e.Key == "Enter" && !string.IsNullOrWhiteSpace(Model.Title))
            {
                await LookupByTitleNowAsync();
            }
        }

        private async Task LookupByTitleNowAsync()
        {
            var query = Model.Title?.Trim();
            if (string.IsNullOrWhiteSpace(query))
                return;

            CancelPendingTitleLookup();
            _titleLookupCts = new CancellationTokenSource();

            try
            {
                await LookupByTitleCoreAsync(query, overwriteTitle: false, _titleLookupCts.Token);
            }
            catch (OperationCanceledException)
            {
            }
        }

        private async Task LookupByTitleCoreAsync(string query, bool overwriteTitle, CancellationToken token)
        {
            _searchingTitle = true;
            _tmdbStatus = null;
            _tmdbStatusIsError = false;
            SafeStateHasChanged();

            try
            {
                var lookup = await TmdbService.SearchMovieSuggestionsAsync(query, maxResults: 5, token);
                token.ThrowIfCancellationRequested();

                if (!string.Equals(Model.Title?.Trim(), query, StringComparison.Ordinal))
                    return;

                if (!lookup.IsSuccess)
                {
                    _suggestions.Clear();
                    _matchedOfficialTitle = null;
                    _tmdbStatus = lookup.ErrorMessage;
                    _tmdbStatusIsError = true;
                    return;
                }

                var results = lookup.Data ?? new List<TmdbSearchResultDto>();
                _suggestions = results;

                if (results.Count == 0)
                {
                    _matchedOfficialTitle = null;
                    _tmdbStatus = $"No TMDB match found for \"{query}\" — you can fill in details manually.";
                    _tmdbStatusIsError = false;
                    return;
                }

                var best = results.FirstOrDefault(r =>
                    string.Equals(r.Title.Trim(), query, StringComparison.OrdinalIgnoreCase)) ?? results[0];

                await ApplyTmdbMovieAsync(best.Id, overwriteTitle, token);
            }
            finally
            {
                _searchingTitle = false;
                SafeStateHasChanged();
            }
        }

        private async Task SelectSuggestionAsync(TmdbSearchResultDto item)
        {
            CancelPendingTitleLookup();
            _searchingTitle = true;
            SafeStateHasChanged();

            try
            {
                await ApplyTmdbMovieAsync(item.Id, overwriteTitle: true);
            }
            finally
            {
                _searchingTitle = false;
                SafeStateHasChanged();
            }
        }

        private async Task HandleTmdbIdKeyDownAsync(KeyboardEventArgs e)
        {
            if (e.Key == "Enter")
            {
                await FetchByTmdbIdAsync();
            }
        }

        private async Task FetchByTmdbIdAsync()
        {
            if (!Model.TmdbId.HasValue || Model.TmdbId.Value <= 0)
            {
                _tmdbStatus = "Please enter a valid positive TMDB ID.";
                _tmdbStatusIsError = true;
                return;
            }

            CancelPendingTitleLookup();
            _fetchingById = true;
            _tmdbStatus = null;
            _tmdbStatusIsError = false;
            SafeStateHasChanged();

            try
            {
                await ApplyTmdbMovieAsync(Model.TmdbId.Value, overwriteTitle: true);
            }
            finally
            {
                _fetchingById = false;
                SafeStateHasChanged();
            }
        }

        private async Task ApplyTmdbMovieAsync(int tmdbId, bool overwriteTitle, CancellationToken token = default)
        {
            var lookup = await TmdbService.GetMovieByIdAsync(tmdbId, token);
            token.ThrowIfCancellationRequested();

            if (!lookup.IsSuccess)
            {
                _matchedOfficialTitle = null;
                _tmdbStatus = lookup.ErrorMessage;
                _tmdbStatusIsError = true;
                return;
            }

            var details = lookup.Data;
            if (details == null)
            {
                _matchedOfficialTitle = null;
                _tmdbStatus = $"No TMDB movie found with ID #{tmdbId}.";
                _tmdbStatusIsError = true;
                return;
            }

            if (overwriteTitle)
                Model.Title = details.Title;

            Model.TmdbId = details.Id;
            Model.Genre = details.Genre;
            Model.ReleaseYear = details.ReleaseYear;
            Model.Description = details.Description;

            if (Model.PosterFile == null)
                Model.PosterUrl = details.PosterUrl;

            Model.IsManual = false;
            _matchedOfficialTitle = details.Title;

            var yearPart = details.ReleaseYear.HasValue ? $" ({details.ReleaseYear.Value})" : string.Empty;
            _tmdbStatus = $"Auto-filled from TMDB: {details.Title}{yearPart} · ID #{details.Id}";
            _tmdbStatusIsError = false;
        }

        private void ApplyOfficialTitle()
        {
            if (!string.IsNullOrWhiteSpace(_matchedOfficialTitle))
                Model.Title = _matchedOfficialTitle;
        }

        private void ClearTmdbMatch()
        {
            CancelPendingTitleLookup();
            Model.TmdbId = null;
            Model.IsManual = true;
            _autoFillOnTitle = false;
            _matchedOfficialTitle = null;
            _suggestions.Clear();
            _tmdbStatus = "Cleared TMDB link. Switched to manual entry.";
            _tmdbStatusIsError = false;
        }

        private void HandleManualEntryChanged(ChangeEventArgs e)
        {
            var isManual = e.Value is bool b
                ? b
                : string.Equals(e.Value?.ToString(), "true", StringComparison.OrdinalIgnoreCase);
            Model.IsManual = isManual;
            _autoFillOnTitle = !isManual;
        }

        private void SafeStateHasChanged()
        {
            if (!_disposed)
                StateHasChanged();
        }

        private void CancelPendingTitleLookup()
        {
            _titleLookupCts?.Cancel();
            _titleLookupCts?.Dispose();
            _titleLookupCts = null;
        }

        public void Dispose()
        {
            _disposed = true;
            CancelPendingTitleLookup();
        }
    }
}
