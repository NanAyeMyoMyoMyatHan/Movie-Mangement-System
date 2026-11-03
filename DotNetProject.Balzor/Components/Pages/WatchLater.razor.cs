using DotNetProject.Balzor.Components.Shared;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetProject.Balzor.Components.Pages
{
    public partial class WatchLater : ComponentBase, IDisposable
    {
        [Inject]
        private WatchLaterService WatchLaterService { get; set; } = default!;

        [Inject]
        private NavigationManager Nav { get; set; } = default!;

        [Inject]
        public AuthService AuthService { get; set; } = default!;

        protected List<WatchLaterItem> Items { get; set; } = new();
        protected List<WatchLaterItem> FilteredItems { get; set; } = new();

        protected string SelectedFilter { get; set; } = "All";
        protected string SortBy { get; set; } = "recent";
        protected string SearchQuery { get; set; } = "";
        protected bool IsLoading { get; set; } = true;
        protected string? ToastMessage { get; set; }

        protected int CountAll => Items.Count;
        protected int CountKorean => Items.Count(i => i.Category != null && i.Category.Contains("Korean", StringComparison.OrdinalIgnoreCase));
        protected int CountThai => Items.Count(i => i.Category != null && i.Category.Contains("Thai", StringComparison.OrdinalIgnoreCase));
        protected int CountMovies => Items.Count(i => !i.IsContent || (i.Category != null && i.Category.Contains("Movie", StringComparison.OrdinalIgnoreCase)));
        protected int CountWatching => Items.Count(i => i.Status == "Watching");

        protected override async Task OnInitializedAsync()
        {
            WatchLaterService.OnChange += HandleServiceChange;
            AuthService.OnAuthStateChanged += HandleAuthChange;
            await LoadItemsAsync();
        }

        private async Task LoadItemsAsync()
        {
            IsLoading = true;
            Items = await WatchLaterService.GetItemsAsync();
            ApplyFilters();
            IsLoading = false;
        }

        protected void SetFilter(string filter)
        {
            SelectedFilter = filter;
            ApplyFilters();
        }

        protected void OnSearchChanged(ChangeEventArgs e)
        {
            SearchQuery = e.Value?.ToString() ?? "";
            ApplyFilters();
        }

        protected void OnSortChanged(ChangeEventArgs e)
        {
            SortBy = e.Value?.ToString() ?? "recent";
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            var query = Items.AsEnumerable();

            // Category filter
            if (SelectedFilter == "Korean Drama")
            {
                query = query.Where(i => i.Category != null && i.Category.Contains("Korean", StringComparison.OrdinalIgnoreCase));
            }
            else if (SelectedFilter == "Thai BL")
            {
                query = query.Where(i => i.Category != null && i.Category.Contains("Thai", StringComparison.OrdinalIgnoreCase));
            }
            else if (SelectedFilter == "Movies")
            {
                query = query.Where(i => !i.IsContent || (i.Category != null && i.Category.Contains("Movie", StringComparison.OrdinalIgnoreCase)));
            }
            else if (SelectedFilter == "Watching")
            {
                query = query.Where(i => i.Status == "Watching");
            }

            // Search query filter
            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                var q = SearchQuery.Trim().ToLower();
                query = query.Where(i => (i.Title != null && i.Title.ToLower().Contains(q)) ||
                                         (i.Description != null && i.Description.ToLower().Contains(q)) ||
                                         (i.Category != null && i.Category.ToLower().Contains(q)));
            }

            // Sorting
            if (SortBy == "title")
            {
                query = query.OrderBy(i => i.Title);
            }
            else if (SortBy == "episodes")
            {
                query = query.OrderByDescending(i => i.TotalEpisodes);
            }
            else
            {
                query = query.OrderByDescending(i => i.AddedAt);
            }

            FilteredItems = query.ToList();
        }

        protected void NavigateToItem(WatchLaterItem item)
        {
            if (item.IsContent)
            {
                Nav.NavigateTo($"/drama/{item.Id}");
            }
            else
            {
                Nav.NavigateTo($"/movie/{item.Id}");
            }
        }

        protected async Task RemoveItem(WatchLaterItem item)
        {
            await WatchLaterService.RemoveItemAsync(item.Id, item.IsContent);
            ShowToast($"Removed \"{item.Title}\" from Watch Later.");
        }

        protected void ExploreCatalog()
        {
            Nav.NavigateTo("/");
        }

        private async void HandleServiceChange()
        {
            await LoadItemsAsync();
            await InvokeAsync(StateHasChanged);
        }

        private void ShowToast(string message)
        {
            ToastMessage = message;
            _ = Task.Run(async () =>
            {
                await Task.Delay(3000);
                ToastMessage = null;
                await InvokeAsync(StateHasChanged);
            });
        }

        private void HandleAuthChange()
        {
            InvokeAsync(StateHasChanged);
        }

        public void Dispose()
        {
            WatchLaterService.OnChange -= HandleServiceChange;
            AuthService.OnAuthStateChanged -= HandleAuthChange;
        }
    }
}
