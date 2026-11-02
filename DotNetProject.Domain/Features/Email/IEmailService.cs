using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetProject.Domain.Features.Email
{
    public interface IEmailService
    {
        Task<SendResponse> SendAsync (string toEmail, string subject, string body);

    }
}
