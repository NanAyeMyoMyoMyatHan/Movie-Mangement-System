using DotNetProject.Balzor.Components.Shared;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetProject.Balzor.Components.Pages
{
    public partial class Favorites : ComponentBase, IDisposable
    {
        [Inject]
        private FavoriteService FavoriteService { get; set; } = default!;

        [Inject]
        private NavigationManager Nav { get; set; } = default!;

        [Inject]
        public AuthService AuthService { get; set; } = default!;

        protected List<FavoriteItem> Items { get; set; } = new();
        protected List<FavoriteItem> FilteredItems { get; set; } = new();

        protected string SelectedFilter { get; set; } = "All";
        protected string SortBy { get; set; } = "rating";
        protected string SearchQuery { get; set; } = "";
        protected bool IsLoading { get; set; } = true;
        protected string? ToastMessage { get; set; }

        protected int CountAll => Items.Count;
        protected int CountKorean => Items.Count(i => i.Category != null && i.Category.Contains("Korean", StringComparison.OrdinalIgnoreCase));
        protected int CountThai => Items.Count(i => i.Category != null && i.Category.Contains("Thai", StringComparison.OrdinalIgnoreCase));
        protected int CountMovies => Items.Count(i => !i.IsContent || (i.Category != null && i.Category.Contains("Movie", StringComparison.OrdinalIgnoreCase)));
        protected double TopRating => Items.Any() ? Items.Max(i => i.Rating) : 9.5;

        protected override async Task OnInitializedAsync()
        {
            FavoriteService.OnChange += HandleServiceChange;
            AuthService.OnAuthStateChanged += HandleAuthChange;
            await LoadItemsAsync();
        }

        private async Task LoadItemsAsync()
        {
            IsLoading = true;
            Items = await FavoriteService.GetFavoritesAsync();
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
            SortBy = e.Value?.ToString() ?? "rating";
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            var query = Items.AsEnumerable();

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

            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                var q = SearchQuery.Trim().ToLower();
                query = query.Where(i => (i.Title != null && i.Title.ToLower().Contains(q)) ||
                                         (i.Description != null && i.Description.ToLower().Contains(q)) ||
                                         (i.Category != null && i.Category.ToLower().Contains(q)));
            }

            if (SortBy == "title")
            {
                query = query.OrderBy(i => i.Title);
            }
            else if (SortBy == "recent")
            {
                query = query.OrderByDescending(i => i.AddedAt);
            }
            else
            {
                query = query.OrderByDescending(i => i.Rating);
            }

            FilteredItems = query.ToList();
        }

        protected void NavigateToItem(FavoriteItem item)
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

        protected async Task RemoveFavorite(FavoriteItem item)
        {
            await FavoriteService.RemoveFavoriteAsync(item.Id, item.IsContent);
            ShowToast($"Removed \"{item.Title}\" from Favorites.");
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

        private async void HandleAuthChange()
        {
            await LoadItemsAsync();
            await InvokeAsync(StateHasChanged);
        }

        public void Dispose()
        {
            FavoriteService.OnChange -= HandleServiceChange;
            AuthService.OnAuthStateChanged -= HandleAuthChange;
        }
    }
}
