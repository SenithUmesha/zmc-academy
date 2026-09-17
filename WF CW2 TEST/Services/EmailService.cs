using System;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;

namespace WF_CW2_TEST.Services
{
    internal static class EmailService
    {
        public static string GenerateVerificationCode()
        {
            var bytes = new byte[4];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }

            uint value = BitConverter.ToUInt32(bytes, 0) % 1000000;
            return value.ToString("D6");
        }

        public static void SendVerificationCode(string recipient, string code)
        {
            string host = Environment.GetEnvironmentVariable("ZMC_SMTP_HOST") ?? "smtp.gmail.com";
            string portValue = Environment.GetEnvironmentVariable("ZMC_SMTP_PORT") ?? "587";
            string user = Environment.GetEnvironmentVariable("ZMC_SMTP_USER");
            string password = Environment.GetEnvironmentVariable("ZMC_SMTP_PASSWORD");
            string from = Environment.GetEnvironmentVariable("ZMC_SMTP_FROM") ?? user;

            if (string.IsNullOrWhiteSpace(user) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(from))
            {
                throw new InvalidOperationException(
                    "SMTP is not configured. Set ZMC_SMTP_USER, ZMC_SMTP_PASSWORD and optionally ZMC_SMTP_FROM.");
            }

            if (!int.TryParse(portValue, out int port))
            {
                throw new InvalidOperationException("ZMC_SMTP_PORT must be a valid integer.");
            }

            using (var mail = new MailMessage())
            using (var smtp = new SmtpClient(host, port))
            {
                mail.From = new MailAddress(from, "ZMC Academy");
                mail.To.Add(recipient);
                mail.Subject = "ZMC Academy verification code";
                mail.Body = "Your ZMC Academy verification code is: " + code;

                smtp.EnableSsl = true;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new NetworkCredential(user, password);
                smtp.Send(mail);
            }
        }
    }
}
