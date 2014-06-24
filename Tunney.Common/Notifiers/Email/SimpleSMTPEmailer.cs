using System;
using System.Net.Mail;
using System.Text;
using System.Collections.Generic;
using System.IO;

namespace Tunney.Common.Notifiers.Email
{
    [Serializable]
    public class SimpleSMTPEmailer : IEmailer
    {
        protected readonly string m_smtpServerName;
        protected readonly IList<string> m_recipentEmailAddresses;
        protected readonly string m_fromAddress;
        protected readonly string m_fromAlias;

        public SimpleSMTPEmailer(string _smtpServerName, string _fromAddress, string _fromAlias, IList<string> _recipentEmailAddresses)
        {
            if (string.IsNullOrEmpty(_smtpServerName))
            {
                throw new ArgumentNullException(@"_smtpServerName");
            }

            if (string.IsNullOrEmpty(_fromAddress))
            {
                throw new ArgumentNullException(@"_fromAddress");
            }

            if (string.IsNullOrEmpty(_fromAlias))
            {
                throw new ArgumentNullException(@"_fromAlias");
            }

            if (null == _recipentEmailAddresses)
            {
                throw new ArgumentNullException(@"_recipentEmailAddresses");
            }

            if (0 == _recipentEmailAddresses.Count)
            {
                throw new ArgumentException(@"Need at least one (1) recipient in order to set up the emailer", @"_recipentEmailAddresses");
            }
            
            m_smtpServerName = _smtpServerName;
            m_fromAddress = _fromAddress;
            m_fromAlias = _fromAlias;
            m_recipentEmailAddresses = _recipentEmailAddresses;
        }

        private SimpleSMTPEmailer(IEmailer _clone)
        {
            if (!(_clone is SimpleSMTPEmailer))
            {
                throw new ArgumentException(@"_clone");
            }

            SimpleSMTPEmailer s = (SimpleSMTPEmailer)_clone;
            m_smtpServerName = s.m_smtpServerName;
            m_recipentEmailAddresses = s.m_recipentEmailAddresses;
            m_fromAlias = s.m_fromAlias;
            m_fromAddress = s.m_fromAddress;
        }

        public static IEmailer Clone(IEmailer _clone)
        {
            SimpleSMTPEmailer retval = new SimpleSMTPEmailer(_clone);
            return retval;
        }

        #region IEmailer Members

        public virtual void Send(string _subject, string _messageBody)
        {
            Send(m_recipentEmailAddresses, new List<string>(), _subject, _messageBody, null);
        }

        public virtual void Send(ICollection<string> _to, ICollection<string> _cc, string _subject, string _messageBody, FileInfo _attachmentFile)
        {
            try
            {
                List<MailAddress> mailAddresses = new List<MailAddress>(_to.Count);
                foreach (string r in _to)
                {
                    if (r.Trim() == string.Empty) continue;
                    mailAddresses.Add(new MailAddress(r));
                }

                List<MailAddress> ccAddresses = new List<MailAddress>(_cc.Count);
                foreach (string r in _cc)
                {
                    if (r.Trim() == string.Empty) continue;
                    ccAddresses.Add(new MailAddress(r));
                }

                MailMessage mm = new MailMessage(new MailAddress(m_fromAddress, m_fromAlias, Encoding.UTF8), mailAddresses[0]);
                mm.IsBodyHtml = true;

                for (int idx = 1; idx < mailAddresses.Count; idx++)//We've added the first one in the ctor, now add the rest!
                {
                    mm.To.Add(mailAddresses[idx]);
                }

                foreach (MailAddress cc in ccAddresses)
                {
                    mm.CC.Add(cc);
                }

                mm.Subject = _subject;
                mm.Body = _messageBody;

                if (null != _attachmentFile)
                {
                    _attachmentFile.Refresh();
                    if (_attachmentFile.Exists)
                    {
                        Attachment file = new Attachment(_attachmentFile.FullName);
                        mm.Attachments.Add(file);
                    }
                }

                using (SmtpClient server = new SmtpClient(m_smtpServerName))
                {
                    server.Send(mm);
                }
            }
            catch// (Exception _ex)
            {
                throw;
            }
        }

        #endregion

        #region ISerializable Members

        private const string SERIALIZATION_SMTP_SERVER = @"SMTPServer";
        private const string SERIALIZATION_FROM_ADDRESS = @"FromAddress";
        private const string SERIALIZATION_FROM_ALIAS = @"FromAlias";
        private const string SERIALIZATION_TO_RECIPENTS = @"ToRecipents";

        protected SimpleSMTPEmailer(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            m_smtpServerName = info.GetString(SERIALIZATION_SMTP_SERVER);
            m_fromAddress = info.GetString(SERIALIZATION_FROM_ADDRESS);
            m_fromAlias = info.GetString(SERIALIZATION_FROM_ALIAS);
            m_recipentEmailAddresses = (IList<string>)info.GetValue(SERIALIZATION_TO_RECIPENTS, typeof(IList<string>));
        }

        public void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            info.AddValue(SERIALIZATION_FROM_ADDRESS, m_fromAddress);
            info.AddValue(SERIALIZATION_FROM_ALIAS, m_fromAlias);
            info.AddValue(SERIALIZATION_SMTP_SERVER, m_smtpServerName);
            info.AddValue(SERIALIZATION_TO_RECIPENTS, m_recipentEmailAddresses);
        }

        #endregion
    }
}