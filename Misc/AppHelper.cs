using System.Net.Mail;
using System.Net;
using System.Text;
using QRCoder;
using System.Drawing;
using System.Drawing.Imaging;
using iTextSharp.text.pdf.qrcode;
using SixLabors.ImageSharp.Formats;
using System.IO;
using B_S_Skyline.Models;
using B_S_Skyline.Services;
using Firebase.Database.Query;
using Firebase.Auth.Providers;
using Firebase.Auth;

namespace B_S_Skyline.Misc
{
    public static class AppHelper
    {
        public static async Task SendEasyPassEmail(string email, string name, Visit visit, string qrCodePath, string projectId)
        {
            string senderEmail = "skylinebns@gmail.com";
            string senderPassword = "rkcm bgte iirz xpiu";

            using (MailMessage mm = new MailMessage(senderEmail, email))
            {
                string projectName = await GetProjectNameAsync(projectId);
                mm.Subject = "Visitor EasyPass";
                mm.IsBodyHtml = true;

                mm.Body = $@"
                    <p>Dear {name},</p>
                    <p>A visitor has requested an EasyPass for their visit to your residence. Here are the details:</p>
                    <ul>
                        <li><strong>Visitor Name:</strong> {visit.VisitorName}</li>
                        <li><strong>License Plate:</strong> {visit.LicensePlate}</li>
                        <li><strong>Visit Date:</strong> {visit.EntryTime?.ToString("f")}</li>
                    </ul>
                    <p>The EasyPass QR code is attached to this email. Please forward it to your visitor.</p>
                    <p>Thank you,<br>The {projectName} Team</p>";

                if (System.IO.File.Exists($"wwwroot{qrCodePath}"))
                {
                    mm.Attachments.Add(new Attachment($"wwwroot{qrCodePath}"));
                }

                SmtpClient smtp = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    EnableSsl = true,
                    Credentials = new NetworkCredential(senderEmail, senderPassword)
                };

                try
                {
                    await smtp.SendMailAsync(mm);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Failed to send EasyPass email: {ex.Message}");
                }
            }
        }

        public static async Task SendRegistrationEmail(string email, string displayName, string password, string qrCodePath, string role, string projectId, string houseNumber)
        {
            string senderEmail = "skylinebns@gmail.com";
            string senderPassword = "rkcm bgte iirz xpiu";

            using (MailMessage mm = new MailMessage(senderEmail, email))
            {
                string projectName = await GetProjectNameAsync(projectId);

                mm.Subject = $"Welcome to {projectName}";
                mm.IsBodyHtml = true;

                mm.Body = $@"
                    <p>Dear {displayName},</p>
                    <p>Welcome to {projectName}! Here are your login details:</p>
                    <ul>
                        <li><strong>Email:</strong> {email}</li>
                        <li><strong>Password:</strong> {password}</li>
                        <li><strong>Project:</strong> {projectName}</li>
                        <li><strong>House Number:</strong> {houseNumber}</li>
                    </ul>

                    <p>Please log in to your account and change your password as soon as possible for added security.</p>
                    <p>You can update your password by clicking the link below:</p>
                    <p><a href='https://localhost:7064/Visits/ChangePassword'>Change Password</a></p>

                    <p>Your QR Code for Easy Pass Access is attached to this email.</p>
                    <p>Thank you,</p>
                    <p>The {projectName} Team</p>";

                if (File.Exists($"wwwroot{qrCodePath}"))
                {
                    mm.Attachments.Add(new Attachment($"wwwroot{qrCodePath}"));
                }

                SmtpClient smtp = new SmtpClient("smtp.gmail.com");
                smtp.EnableSsl = true;
                smtp.Port = 587;
                smtp.Credentials = new NetworkCredential(senderEmail, senderPassword);

                try
                {
                    smtp.Send(mm);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Failed to send email: {ex.Message}");
                }
            }
        }
        private static async Task<string> GetProjectNameAsync(string projectId)
        {
            var firebaseClient = FirebaseService.GetFirebaseClient();
            var project = await firebaseClient
                .Child("projects")
                .Child(projectId)
                .OnceSingleAsync<ResidentialProject>();

            return project?.Name ?? "Unknown Project";
        }
    }

    public static class QRHelper
    {
        public static string GenerateQRCode(string residentId)
        {
            string content = $"RESIDENT:{residentId}";

            string directory = "wwwroot/qr/";
            string path = $"{directory}{residentId}.png";

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);

            PngByteQRCode qrCode = new(qrCodeData);
            byte[] qrCodeImage = qrCode.GetGraphic(20);

            using (var ms = new MemoryStream(qrCodeImage))
            using (var bitmap = new Bitmap(ms))
            {
                bitmap.Save(path, ImageFormat.Png);
            }

            return path.Replace("wwwroot", string.Empty);
        }
    }
}
