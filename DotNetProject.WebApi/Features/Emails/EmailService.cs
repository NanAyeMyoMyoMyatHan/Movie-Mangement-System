using DotNetProject.Domain.Features.Email;
using FluentEmail.Core;

namespace DotNetProject.WebApi.Features.Emails
{
    public class EmailService : IEmailService
    {
        private IFluentEmail _fluentEmail;

        public EmailService(IFluentEmail fluentEmail)
        {
            _fluentEmail = fluentEmail;
        }
        public async Task<SendResponse> SendAsync(string toEmail, string subject, string body)
        {
            var response = await _fluentEmail
                .To(toEmail)
                .Subject(subject)
                .Body(body)
                .SendAsync();

            return new SendResponse
            {
                IsSuccess = response.Successful,
                
                MessageId = response.MessageId,
                Errors = response.ErrorMessages
            };
        }
    }
}
