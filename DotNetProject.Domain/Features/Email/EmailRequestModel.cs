using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetProject.Domain.Features.Email
{
    public class EmailRequestModel
    {
        public string ToEmail { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; }=string.Empty;
    }
    public class SendResponse
    {
        // အီးမေးလ် ပို့တာ အောင်မြင်ခြင်း ရှိမရှိ (True/False)
        public bool IsSuccess { get; set; }

        // အောင်မြင်ကြောင်း သို့မဟုတ် အမှား (Error) ဖြစ်ကြောင်း ဖော်ပြမယ့် စာသား
        public string Message { get; set; } = string.Empty;

        // Email Provider (SendGrid, Mailgun, SMTP စသည်) က ပြန်ပေးလိုက်တဲ့ Message ID
        public string? MessageId { get; set; }

        // Errors တွေ ရှိရင် စာရင်းပြုစုဖို့ (Optional)
        public IEnumerable<string> Errors { get; set; } = new List<string>();
    }
}
