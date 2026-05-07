using System;
using System.Collections.Generic;
using System.Text;

namespace Arclight.Application.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(List<string> bccEmails, string subject, string body);
    }
}
