using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNetProject.Balzor.Components.Shared;
using DotNetProject.Domain.Features.Audit;
using Microsoft.AspNetCore.Components;

namespace DotNetProject.Balzor.Components.Pages.Admin
{
    public partial class AdminAuditLogs
    {
        [Inject]
        public AuthService AuthService { get; set; } = default!;

        [Inject]
        public AuditLogService AuditLogService { get; set; } = default!;

        [Inject]
        public NavigationManager Nav { get; set; } = default!;

        protected bool IsLoading { get; set; } = true;
        protected string? ErrorMessage { get; set; }

        protected PagedAuditLogResult LogResult { get; set; } = new();
        protected AuditLogFilterRequest Filter { get; set; } = new()
        {
            Action = "All",
            TargetEntity = "All",
            PageNumber = 1,
            PageSize = 20
        };

        protected DateTime? FromDateInput { get; set; }
        protected DateTime? ToDateInput { get; set; }

        // Stats
        protected int TotalEntries => LogResult.TotalCount;
        protected int SecurityMutationsCount { get; set; }
        protected int ContentMutationsCount { get; set; }

        protected override async Task OnInitializedAsync()
        {
            if (!AuthService.IsAuthenticated() || !AuthService.CanAccessAdmin())
            {
                Nav.NavigateTo("/login?returnUrl=/admin/audit-logs");
                return;
            }

            await LoadLogsAsync();
        }

        protected async Task LoadLogsAsync()
        {
            IsLoading = true;
            ErrorMessage = null;

            try
            {
                Filter.FromDate = FromDateInput;
                Filter.ToDate = ToDateInput.HasValue ? ToDateInput.Value.Date.AddDays(1).AddTicks(-1) : null;

                LogResult = await AuditLogService.GetLogsAsync(Filter);

                // Calculate summary counters for current page/result
                SecurityMutationsCount = LogResult.Items.Count(i => 
                    i.Action.Contains("ROLE", StringComparison.OrdinalIgnoreCase) || 
                    i.Action.Contains("USER", StringComparison.OrdinalIgnoreCase) ||
                    i.Action.Contains("PASSWORD", StringComparison.OrdinalIgnoreCase));

                ContentMutationsCount = LogResult.Items.Count(i => 
                    i.Action.Contains("CONTENT", StringComparison.OrdinalIgnoreCase) || 
                    i.Action.Contains("MOVIE", StringComparison.OrdinalIgnoreCase) ||
                    i.Action.Contains("EPISODE", StringComparison.OrdinalIgnoreCase));
            }
            catch (Exception ex)
            {
                ErrorMessage = "Failed to load audit logs: " + ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }

        protected async Task ApplyFilter()
        {
            Filter.PageNumber = 1;
            await LoadLogsAsync();
        }

        protected async Task SetActionFilter(string action)
        {
            Filter.Action = action;
            Filter.PageNumber = 1;
            await LoadLogsAsync();
        }

        protected async Task ResetFilters()
        {
            Filter.Action = "All";
            Filter.UserName = string.Empty;
            Filter.TargetEntity = "All";
            FromDateInput = null;
            ToDateInput = null;
            Filter.PageNumber = 1;
            await LoadLogsAsync();
        }

        protected async Task GoToPage(int page)
        {
            if (page < 1 || page > LogResult.TotalPages || page == Filter.PageNumber)
                return;

            Filter.PageNumber = page;
            await LoadLogsAsync();
        }

        protected async Task OnPageSizeChange(ChangeEventArgs e)
        {
            if (int.TryParse(e.Value?.ToString(), out int newSize))
            {
                Filter.PageSize = newSize;
                Filter.PageNumber = 1;
                await LoadLogsAsync();
            }
        }

        protected string GetActionBadgeClass(string action)
        {
            var upper = action.ToUpperInvariant();
            if (upper.StartsWith("DELETE")) return "badge-danger";
            if (upper.StartsWith("CREATE") || upper.StartsWith("INSERT")) return "badge-success";
            if (upper.StartsWith("UPDATE") || upper.StartsWith("EDIT")) return "badge-info";
            if (upper.Contains("ROLE") || upper.Contains("ACCESS") || upper.Contains("USER")) return "badge-warning";
            return "badge-default";
        }
    }
}
