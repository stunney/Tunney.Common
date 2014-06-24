using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;

namespace Tunney.Common.Notifiers.Email
{
    public interface IEmailer : ISerializable
    {
        void Send(string _subject, string _messageBody);
        void Send(ICollection<string> _to, ICollection<string> _cc, string _subject, string _messageBody, FileInfo _attachmentFilename);
    }
}