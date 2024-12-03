using Microsoft.AspNetCore.Identity.UI.Services;
using System.Threading.Tasks;

public class EmailSender : IEmailSender
{
    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        // Tutaj dodaj logikę wysyłania e-maili, np. za pomocą SMTP
        Console.WriteLine($"Sending email to {email} with subject {subject}");
        return Task.CompletedTask;
    }
}

