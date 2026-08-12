using DotNetProject.WindowForm.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetProject.WindowForm
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            var services = new ServiceCollection();
            services.AddHttpClient();
            services.AddTransient<HttpClientServices>();
            services.AddTransient<Form1>();
            services.AddTransient<LoginForm>();
            services.AddTransient<RegisterForm>();
            var serviceProvider = services.BuildServiceProvider();

            // Show the login form first. Only open the main window if login succeeds.
            var loginForm = serviceProvider.GetRequiredService<LoginForm>();
            if (loginForm.ShowDialog() == DialogResult.OK)
            {
                var mainForm = serviceProvider.GetRequiredService<Form1>();
                mainForm.CurrentUser = loginForm.LoggedInUser;
                Application.Run(mainForm);
            }
        }
    }
}