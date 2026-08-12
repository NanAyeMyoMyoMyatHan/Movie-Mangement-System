using System;
using System.Drawing;
using System.Windows.Forms;
using DotNetProject.WebApi.Features.Users;
using DotNetProject.WindowForm.Services;

namespace DotNetProject.WindowForm
{
    internal partial class RegisterForm : Form
    {
        private const string ApiBaseUrl = "http://localhost:5134/api";

        private readonly HttpClientServices _httpClientServices;

        public string RegisteredEmail { get; private set; }

        public RegisterForm(HttpClientServices httpClientServices)
        {
            InitializeComponent();
            _httpClientServices = httpClientServices;

            btnRegister.Click += BtnRegister_Click;
            lnkGoToLogin.LinkClicked += LnkGoToLogin_LinkClicked;
        }

        private async void BtnRegister_Click(object sender, EventArgs e)
        {
            lblStatus.ForeColor = Color.Crimson;
            lblStatus.Text = "";

            var userName = txtRegUserName.Text.Trim();
            var displayName = txtRegDisplayName.Text.Trim();
            var email = txtRegEmail.Text.Trim();
            var password = txtRegPassword.Text;
            var confirmPassword = txtRegConfirmPassword.Text;

            if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password))
            {
                lblStatus.Text = "Username, email and password are required.";
                return;
            }
            if (password != confirmPassword)
            {
                lblStatus.Text = "Passwords do not match.";
                return;
            }

            btnRegister.Enabled = false;
            btnRegister.Text = "Creating account...";
            try
            {
                var request = new UserCreateRequestModel
                {
                    UserName = userName,
                    DisplayName = string.IsNullOrWhiteSpace(displayName) ? userName : displayName,
                    Email = email,
                    Password = password
                };

                var result = await _httpClientServices.ExectueAsync<UserCreateResponseModel>(
                    $"{ApiBaseUrl}/User", request, EnumHttpMethod.Post);

                if (result != null && result.IsSuccess)
                {
                    RegisteredEmail = email;
                    DialogResult = DialogResult.OK;
                    Close();
                }
                else
                {
                    lblStatus.Text = result?.Message ?? "Could not create the account.";
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Could not reach the server: " + ex.Message;
            }
            finally
            {
                btnRegister.Enabled = true;
                btnRegister.Text = "Sign Up";
            }
        }

        private void LnkGoToLogin_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
