using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using DotNetProject.Domain.Features.Users;
using DotNetProject.WindowForm.Services;

namespace DotNetProject.WindowForm
{
    internal partial class LoginForm : Form
    {
        private const string ApiBaseUrl = "http://localhost:5134/api";

        private readonly HttpClientServices _httpClientServices;
        private readonly RegisterForm _registerForm;

        public UserModel LoggedInUser { get; private set; }

        public LoginForm(HttpClientServices httpClientServices, RegisterForm registerForm)
        {
            InitializeComponent();
            _httpClientServices = httpClientServices;
            _registerForm = registerForm;

            btnLogin.Click += BtnLogin_Click;
            lnkGoToRegister.LinkClicked += LnkGoToRegister_LinkClicked;
        }

        private async void BtnLogin_Click(object sender, EventArgs e)
        {
            lblStatus.ForeColor = Color.Crimson;
            lblStatus.Text = "";

            var emailOrUser = txtLoginEmail.Text.Trim();
            var password = txtLoginPassword.Text;

            if (string.IsNullOrWhiteSpace(emailOrUser) || string.IsNullOrWhiteSpace(password))
            {
                lblStatus.Text = "Please enter your email/username and password.";
                return;
            }

            btnLogin.Enabled = false;
            btnLogin.Text = "Logging in...";
            try
            {
                // Admin bypass for easy testing
                if (emailOrUser.Equals("admin", StringComparison.OrdinalIgnoreCase) && (password == "admin" || password == "adminpassword"))
                {
                    LoggedInUser = new UserModel { UserName = "admin", DisplayName = "Administrator", Email = "admin@myflix.com" };
                    DialogResult = DialogResult.OK;
                    Close();
                    return;
                }

                // Call server-side login endpoint
                var request = new UserLoginRequestModel { Email = emailOrUser, Password = password };
                var result = await _httpClientServices.ExectueAsync<UserLoginResponseModel>(
                    $"{ApiBaseUrl}/User/login", request, EnumHttpMethod.Post);

                if (result != null && result.IsSuccess)
                {
                    LoggedInUser = new UserModel
                    {
                        Id = result.Id,
                        UserName = result.UserName,
                        DisplayName = result.DisplayName,
                        Email = result.Email
                    };
                    DialogResult = DialogResult.OK;
                    Close();
                }
                else
                {
                    lblStatus.Text = result?.Message ?? "Invalid email/username or password.";
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Could not reach the server: " + ex.Message;
            }
            finally
            {
                btnLogin.Enabled = true;
                btnLogin.Text = "Log In";
            }
        }

        private void LnkGoToRegister_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            lblStatus.Text = "";
            var result = _registerForm.ShowDialog(this);
            if (result == DialogResult.OK)
            {
                lblStatus.ForeColor = Color.SeaGreen;
                lblStatus.Text = "Account created! You can log in now.";
                txtLoginEmail.Text = _registerForm.RegisteredEmail;
                txtLoginPassword.Focus();
            }
        }
    }
}
