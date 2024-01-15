using System.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace BlazorTemplate.API.Utility;

//The details in this github issue where helpful.
//https://github.com/dotnet/aspnetcore/issues/50298

public class AuthMessageSenderOptions
{
    public string? SendGridKey { get; set; }
}

internal sealed class EmailSender : IEmailSender<IdentityUser>
{
    private IConfiguration _configuration { get; }
    public AuthMessageSenderOptions Options { get; }
    private readonly ILogger _logger;
    private IHttpContextAccessor _httpContextAccessor;

    public EmailSender(IOptions<AuthMessageSenderOptions> optionsAccessor, IConfiguration configuration,
                       ILogger<EmailSender> logger, IHttpContextAccessor httpContextAccessor)
    {
        _configuration = configuration;
        Options = optionsAccessor.Value;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    private async Task SendEmailAsync(string toEmail, string subject, string message)
    {
        if (string.IsNullOrEmpty(Options.SendGridKey))
        {
            return;
        }
        if(string.IsNullOrEmpty(_configuration.GetValue<string>("SendGridSystemEmailAddress")))
        {
            return;
        }
        await Execute(Options.SendGridKey, subject, message, toEmail);
    }

    private async Task Execute(string apiKey, string subject, string message, string toEmail)
    {
        if (string.IsNullOrEmpty(Options.SendGridKey))
        {
            return;
        }
        if(string.IsNullOrEmpty(_configuration.GetValue<string>("SendGridSystemEmailAddress")))
        {
            return;
        }

        var client = new SendGridClient(apiKey);
        var msg = new SendGridMessage()
        {
            From = new EmailAddress(_configuration.GetValue<string>("SendGridSystemEmailAddress")),
            Subject = subject,
            PlainTextContent = message,
            HtmlContent = message
        };
        msg.AddTo(new EmailAddress(toEmail));

        // Disable click tracking.
        // See https://sendgrid.com/docs/User_Guide/Settings/tracking.html
        msg.SetClickTracking(false, false);
        var response = await client.SendEmailAsync(msg);
        _logger.LogInformation(response.IsSuccessStatusCode 
                               ? $"Email to {toEmail} queued successfully!"
                               : $"Failure Email to {toEmail}");
    }

    //This is the stock method.
    // public Task SendConfirmationLinkAsync(IdentityUser user, string email, string confirmationLink) =>
    //     SendEmailAsync(email, "Confirm your email", $"Please confirm your account by <a href='{confirmationLink}'>clicking here</a>. Thank you.");
    public Task SendConfirmationLinkAsync(IdentityUser user, string email, string confirmationLink)
    {
        //Adjust the confirmEmail link to use a link in the App, instead of in the API. Keeps the user flow in the App.
        Uri confirmationLinkUri = new Uri(confirmationLink);
        var adjustedUrlForApp = confirmationLinkUri.PathAndQuery.Replace("confirmEmail","confirmingEmail");
        var hostUrl = _httpContextAccessor.HttpContext.Request.Host.Value;

        Uri adjustedConfirmationLink = new Uri($"https://{hostUrl}{adjustedUrlForApp}");


        if(adjustedConfirmationLink.Query.Contains("changedEmail="))
        {
            return SendEmailAsync(email, "Confirm your changed email", $"You have changed your email. Please confirm your new email by <a href='{adjustedConfirmationLink}'>clicking here</a>. If you did not request an email change, disregard this email. Thank you.");
        }
        else
        {
            return SendEmailAsync(email, "Confirm your email", $"Please confirm your account by <a href='{adjustedConfirmationLink}'>clicking here</a>. Thank you.");
        }
    }
 

    public Task SendPasswordResetLinkAsync(IdentityUser user, string email, string resetLink) =>
        SendEmailAsync(email, "Reset your password", $"Please reset your password by <a href='{resetLink}'>clicking here</a>.");
 
    public Task SendPasswordResetCodeAsync(IdentityUser user, string email, string resetCode)
    {
        var resetLink = $"https://{_httpContextAccessor.HttpContext.Request.Host.Value}/password/reset/{resetCode}";
        return SendEmailAsync(email, "Reset your password", $"Please reset your password using the following link <a href='{resetLink}'>Reset Password.</a>");
    }
}

public class EmailSenderGeneric : IEmailSender
{
    private IConfiguration _configuration { get; }
    public AuthMessageSenderOptions Options { get; }
    private readonly ILogger _logger;

    public EmailSenderGeneric(IOptions<AuthMessageSenderOptions> optionsAccessor, IConfiguration configuration,
                       ILogger<EmailSenderGeneric> logger)
    {
        _configuration = configuration;
        Options = optionsAccessor.Value;
        _logger = logger;
    }


    public async Task SendEmailAsync(string toEmail, string subject, string message)
    {
        if (string.IsNullOrEmpty(Options.SendGridKey))
        {
            throw new Exception("Null SendGridKey");
        }
        await Execute(Options.SendGridKey, subject, message, toEmail);
    }

    public async Task Execute(string apiKey, string subject, string message, string toEmail)
    {
        if (string.IsNullOrEmpty(Options.SendGridKey))
        {
            return;
        }
        if(string.IsNullOrEmpty(_configuration.GetValue<string>("SendGridSystemEmailAddress")))
        {
            return;
        }

        var client = new SendGridClient(apiKey);
        var msg = new SendGridMessage()
        {
            From = new EmailAddress(_configuration.GetValue<string>("SendGridSystemEmailAddress")),
            Subject = subject,
            PlainTextContent = message,
            HtmlContent = message
        };
        msg.AddTo(new EmailAddress(toEmail));

        // Disable click tracking.
        // See https://sendgrid.com/docs/User_Guide/Settings/tracking.html
        msg.SetClickTracking(false, false);
        var response = await client.SendEmailAsync(msg);
        _logger.LogInformation(response.IsSuccessStatusCode 
                               ? $"Email to {toEmail} queued successfully!"
                               : $"Failure Email to {toEmail}");
    }
}