using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;
using Newtonsoft.Json;

namespace NitroLaser.Controllers
{
    [RoutePrefix("api")]
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class MainController : ApiController
    {
        [Route("ping")]
        [HttpGet]
        public string Ping()
        {
            return "pong";
        }

        [Route("template")]
        [HttpPost]
        public async Task<IHttpActionResult> SendTemplate()
        {
            var result = await Request.Content.ReadAsMultipartAsync();
            var emailContent = new EmailContent();
            EmailAttachment emailAttachment = null;

            foreach (HttpContent content in result.Contents)
            {
                switch (content.Headers.ContentDisposition.Name.Trim('\"'))
                {
                    case "name":
                        emailContent.Name = content.ReadAsStringAsync().Result;
                        break;

                    case "company":
                        emailContent.Company = content.ReadAsStringAsync().Result;
                        break;

                    case "phone":
                        emailContent.Phone = content.ReadAsStringAsync().Result;
                        break;

                    case "email":
                        emailContent.Email = content.ReadAsStringAsync().Result;
                        break;

                    case "subject":
                        emailContent.Subject = content.ReadAsStringAsync().Result;
                        break;

                    case "msg":
                        emailContent.Message = content.ReadAsStringAsync().Result;
                        break;

                    case "file":
                        emailAttachment = new EmailAttachment();
                        emailAttachment.FileName = content.Headers.ContentDisposition.FileName;
                        emailAttachment.File = content.ReadAsStreamAsync().Result;
                        break;
                }
            }

            SendMail(emailContent, emailAttachment);

            return Ok();
        }

        static void SendMail(EmailContent content, EmailAttachment attachment)
        {
            try
            {
                StringBuilder builder = new StringBuilder();
                builder.AppendLine("نام: " + content.Name);
                builder.AppendLine("شرکت: " + content.Company);
                builder.AppendLine("شماره تماس: " + content.Phone);
                builder.AppendLine("ایمیل: " + content.Email);
                builder.AppendLine("موضوع: " + content.Subject);
                builder.AppendLine("پیام: " + content.Message);

                // Create the mail message
                MailMessage mail = new MailMessage();

                //mail.From = new MailAddress(string.IsNullOrEmpty(content.Email) ? "no-email@nitrolaser.ir" : content.Email);
                mail.From = new MailAddress("info@delta-persia.com");
                mail.To.Add(new MailAddress("info@delta-persia.com"));
                //mail.Bcc.Add(new MailAddress("Three@gmail.com"));
                
                mail.BodyEncoding = Encoding.UTF8;
                mail.Subject = "قالب جدید";
                mail.Body = builder.ToString();
                //mail.IsBodyHtml = true;
                //mail.Priority = MailPriority.High;
                if (attachment != null)
                {
                    Attachment at = new Attachment(attachment.File, attachment.FileName.Trim('\"'));
                    mail.Attachments.Add(at);
                } 

                //prepare to send mail via SMTP transport
                SmtpClient smtp = new SmtpClient();
                smtp.Host = "delta-persia.com";
                smtp.Port = 25;
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;

                System.Net.NetworkCredential credential = new System.Net.NetworkCredential();

                credential.UserName = "info@delta-persia.com";
                credential.Password = "1234@abcd";
                smtp.EnableSsl = false;
                smtp.Credentials = credential;
                smtp.Send(mail);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                attachment?.File.Close();
            }
        }
    }

    class EmailContent
    {
        public string Name { get; set; }
        public string Company { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; } 
    }

    class EmailAttachment
    {
        public Stream File { get; set; }
        public string FileName { get; set; }
    }

}
