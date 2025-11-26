using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using RaquelMenopausa.Cms.Models.Dto;

namespace RaquelMenopausa.Cms.Helpers
{
    public class EmailService
    {
        private readonly SmtpDto _settings;

        public EmailService(IConfiguration configuration)
        {
            _settings = configuration.GetSection("Smtp").Get<SmtpDto>();
        }

        public async Task<bool> EnviarEmailAsync(string destinatario, string assunto, string mensagemHtml)
        {
            try
            {
                var mail = new MailMessage()
                {
                    From = new MailAddress(_settings.Email, "Sistema - Notificação"),
                    Subject = assunto,
                    Body = mensagemHtml,
                    IsBodyHtml = true
                };

                mail.To.Add(destinatario);

                using (var smtp = new SmtpClient(_settings.Host, _settings.Port))
                {
                    smtp.Credentials = new NetworkCredential(_settings.Email, _settings.Senha);
                    smtp.EnableSsl = true;

                    await smtp.SendMailAsync(mail);
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro ao enviar email: " + ex.Message);
                return false;
            }
        }
    }
}
