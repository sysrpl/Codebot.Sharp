using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MimeKit;

namespace Codebot.Mail
{
    public class MailMessage
    {
        Mailbox from = new Mailbox();
        Mailbox to = new Mailbox();
        Mailbox[] copyTo = new Mailbox[0];
        Mailbox[] blindCopyTo = new Mailbox[0];

        readonly string server;
        readonly string login;
        readonly string password;

        public MailMessage(string server, string login, string password)
        {
            this.server = server;
            this.login = login;
            this.password = password;
        }

        public Mailbox From { get => from; set => from.Copy(value); }
        public Mailbox To { get => to; set => to.Copy(value); }
        public string Subject { get; set; }
        public string Body { get; set; }

        public void CopyTo(IEnumerable<Mailbox> recipients)
        {
            copyTo = recipients.ToArray();
        }

        public void BlindCopyTo(IEnumerable<Mailbox> recipients)
        {
            blindCopyTo = recipients.ToArray();
        }

        public void Reset()
        {
            From.Address = "";
            From.Name = "";
            To.Address = "";
            To.Name = "";
            copyTo = new Mailbox[0];
            blindCopyTo = new Mailbox[0];
        }

        static async Task Send(MimeMessage message, string server, 
            string login, string password, bool async)
        {
            void f()
            {
                using (var client = new SmtpClient())
                {
                    client.Connect(server, 465, true);
                    client.Authenticate(login, password);
                    client.Send(message);
                    client.Disconnect(true);
                }
            }
            if (async)
                await Task.Run((Action)f);
            else
                f();
        }

        async Task Send(bool async)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(From.Name, From.Address));
            message.To.Add(new MailboxAddress(To.Name, To.Address));
            foreach (var a in copyTo)
                message.Cc.Add(new MailboxAddress(a.Name, a.Address));
            foreach (var a in blindCopyTo)
                message.Bcc.Add(new MailboxAddress(a.Name, a.Address));
            message.Subject = Subject;
            var builder = new BodyBuilder { TextBody = Body };
            message.Body = builder.ToMessageBody();
            await Send(message, server, login, password, async);
        }

        public void Send()
        {
            Send(false).Wait();
        }

        public async Task SendAsync()
        {
            await Send(true);
        }

        static async Task Send(string from, string to,
            string subject, string body, string server,
            string login, string password, bool async)
        {
            var message = new MimeMessage { Subject = subject };
            var s = from.Split(',');
            var address = s.Length > 1 ?
                new MailboxAddress(s[0].Trim(), s[1].Trim()) :
                new MailboxAddress(s[0].Trim());
            message.From.Add(address);
            s = to.Split(',');
            address = s.Length > 1 ?
                new MailboxAddress(s[0].Trim(), s[1].Trim()) :
                new MailboxAddress(s[0].Trim());
            message.To.Add(address);
            var builder = new BodyBuilder { TextBody = body };
            message.Body = builder.ToMessageBody();
            await Send(message, server, login, password, async);
        }

        public static void Send(string from, string to,
            string subject, string body, string server,
            string login, string password)
        {
            Send(from, to, subject, body, server, login, password, false).Wait();
        }

        public static async Task SendAsync(string from, string to,
            string subject, string body, string server,
            string login, string password)
        {
            await Send(from, to, subject, body, server, login, password, false);
        }
    }
}
