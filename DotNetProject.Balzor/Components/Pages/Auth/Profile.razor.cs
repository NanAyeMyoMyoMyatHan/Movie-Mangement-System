using System;
using System.IO;
using System.Threading.Tasks;
using DotNetProject.Balzor.Components.Shared;
using DotNetProject.Domain.Features.Users;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

namespace DotNetProject.Balzor.Components.Pages.Auth
{
    public partial class Profile : ComponentBase
    {
        [Inject]
        public AuthService AuthService { get; set; } = default!;

        [Inject]
        public HttpClientService HttpClientService { get; set; } = default!;

        [Inject]
        public NavigationManager Nav { get; set; } = default!;

        [Inject]
        public IJSRuntime JS { get; set; } = default!;

        protected UserProfileDto? UserProfile { get; set; }
        protected string? DisplayNameInput { get; set; }
        protected string? BioInput { get; set; }
        protected string? PreviewAvatarUrl { get; set; }

        protected string CurrentPasswordInput { get; set; } = "";
        protected string NewPasswordInput { get; set; } = "";
        protected string ConfirmNewPasswordInput { get; set; } = "";

        protected string? StatusMessage { get; set; }
        protected bool IsStatusSuccess { get; set; } = true;

        protected bool IsSavingProfile { get; set; }
        protected bool IsChangingPassword { get; set; }
        protected bool IsUploadingAvatar { get; set; }

        protected override async Task OnInitializedAsync()
        {
            if (AuthService.IsAuthenticated())
            {
                await LoadProfileAsync();
            }
        }

        protected override void OnAfterRender(bool firstRender)
        {
            if (firstRender && !AuthService.IsAuthenticated())
            {
                Nav.NavigateTo("/login?returnUrl=/profile");
            }
        }

        private async Task LoadProfileAsync()
        {
            try
            {
                int userId = AuthService.CurrentUser.Id;
                if (userId > 0)
                {
                    UserProfile = await HttpClientService.GetUserProfileAsync(userId);
                }

                if (UserProfile == null)
                {
                    UserProfile = new UserProfileDto
                    {
                        Id = AuthService.CurrentUser.Id,
                        UserName = AuthService.CurrentUser.UserName,
                        DisplayName = AuthService.CurrentUser.DisplayName,
                        Email = AuthService.CurrentUser.Email,
                        Role = AuthService.CurrentUser.Role,
                        CreatedDate = DateTime.UtcNow
                    };
                }

                DisplayNameInput = UserProfile.DisplayName;
                BioInput = UserProfile.Bio;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Profile] Error loading user profile: {ex.Message}");
            }
        }

        protected async Task HandleAvatarFileSelected(InputFileChangeEventArgs e)
        {
            var file = e.File;
            if (file == null) return;

            const long maxFileSize = 5 * 1024 * 1024; // 5 MB
            if (file.Size > maxFileSize)
            {
                StatusMessage = "Profile photo must be smaller than 5 MB.";
                IsStatusSuccess = false;
                return;
            }

            IsUploadingAvatar = true;
            StatusMessage = null;
            StateHasChanged();

            try
            {
                using var stream = file.OpenReadStream(maxFileSize);
                using var ms = new MemoryStream();
                await stream.CopyToAsync(ms);
                ms.Position = 0;

                int userId = AuthService.CurrentUser.Id;
                var uploadedUrl = await HttpClientService.UploadAvatarAsync(userId, ms, file.Name);

                if (!string.IsNullOrWhiteSpace(uploadedUrl))
                {
                    PreviewAvatarUrl = uploadedUrl;
                    if (UserProfile != null) UserProfile.AvatarUrl = uploadedUrl;
                    AuthService.UpdateAvatar(uploadedUrl);

                    StatusMessage = "Profile photo uploaded successfully!";
                    IsStatusSuccess = true;
                }
                else
                {
                    // Fallback to local base64 preview for immediate feedback if WebApi upload fails
                    ms.Position = 0;
                    var base64 = Convert.ToBase64String(ms.ToArray());
                    PreviewAvatarUrl = $"data:{file.ContentType};base64,{base64}";
                    StatusMessage = "Profile photo updated locally!";
                    IsStatusSuccess = true;
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Profile photo upload failed: {ex.Message}";
                IsStatusSuccess = false;
            }
            finally
            {
                IsUploadingAvatar = false;
                StateHasChanged();
            }
        }

        protected async Task SaveProfileAsync()
        {
            if (UserProfile == null) return;

            IsSavingProfile = true;
            StatusMessage = null;
            StateHasChanged();

            try
            {
                var req = new UserProfileUpdateRequestModel
                {
                    UserId = UserProfile.Id,
                    DisplayName = DisplayNameInput?.Trim(),
                    Bio = BioInput?.Trim()
                };

                var res = await HttpClientService.UpdateUserProfileAsync(req);
                if (res != null && res.IsSuccess)
                {
                    if (res.User != null)
                    {
                        UserProfile = res.User;
                        DisplayNameInput = res.User.DisplayName;
                        BioInput = res.User.Bio;
                    }
                    StatusMessage = "Profile updated successfully!";
                    IsStatusSuccess = true;
                }
                else
                {
                    StatusMessage = res?.Message ?? "Failed to save profile changes.";
                    IsStatusSuccess = false;
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error saving profile: {ex.Message}";
                IsStatusSuccess = false;
            }
            finally
            {
                IsSavingProfile = false;
                StateHasChanged();
            }
        }

        protected async Task ChangePasswordAsync()
        {
            if (string.IsNullOrWhiteSpace(CurrentPasswordInput))
            {
                StatusMessage = "Please enter your current password.";
                IsStatusSuccess = false;
                return;
            }

            if (string.IsNullOrWhiteSpace(NewPasswordInput) || NewPasswordInput.Length < 6)
            {
                StatusMessage = "New password must be at least 6 characters long.";
                IsStatusSuccess = false;
                return;
            }

            if (NewPasswordInput != ConfirmNewPasswordInput)
            {
                StatusMessage = "New password and confirmation password do not match.";
                IsStatusSuccess = false;
                return;
            }

            IsChangingPassword = true;
            StatusMessage = null;
            StateHasChanged();

            try
            {
                var req = new ChangePasswordRequestModel
                {
                    UserId = AuthService.CurrentUser.Id,
                    CurrentPassword = CurrentPasswordInput.Trim(),
                    NewPassword = NewPasswordInput.Trim(),
                    ConfirmNewPassword = ConfirmNewPasswordInput.Trim()
                };

                var res = await HttpClientService.ChangeUserPasswordAsync(req);
                if (res != null && res.IsSuccess)
                {
                    StatusMessage = "Password changed successfully! Keep your new password secure.";
                    IsStatusSuccess = true;
                    CurrentPasswordInput = "";
                    NewPasswordInput = "";
                    ConfirmNewPasswordInput = "";
                }
                else
                {
                    StatusMessage = res?.Message ?? "Failed to change password.";
                    IsStatusSuccess = false;
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error changing password: {ex.Message}";
                IsStatusSuccess = false;
            }
            finally
            {
                IsChangingPassword = false;
                StateHasChanged();
            }
        }
    }
}
