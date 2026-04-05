using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace back_end.Components;

public class MailSender
{
    private readonly string _smtpHost;
    private readonly int _smtpPort;
    private readonly string _smtpUser;
    private readonly string _smtpPass;
    private readonly RazorService _razorService;
    
    public MailSender(string host, int port, string user, string pass, RazorService razorService)
    {
        _smtpHost = host;
        _smtpPort = port;
        _smtpUser = user;
        _smtpPass = pass;
        _razorService = razorService;
    }
    
    public async Task SendCodeEmailAsync(string toEmail, string username, string link)
    {
        var html = await _razorService.RenderAsync("Email/VerifyEmail.cshtml", new
        {
            Username = username,
            Link = link
        });
        var mailMessage = new MimeMessage();
        mailMessage.From.Add(new MailboxAddress("Étudify", _smtpUser));
        mailMessage.To.Add(new MailboxAddress("", toEmail));
        mailMessage.Subject = "Email Verification";
        mailMessage.Body = new TextPart("html")
        {
            Text = html
        };

        using var client = new SmtpClient();
        await client.ConnectAsync(_smtpHost, _smtpPort, SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(_smtpUser, _smtpPass);
        await client.SendAsync(mailMessage);
        await client.DisconnectAsync(true);
    }
}