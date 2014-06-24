using System;
using Tunney.Common.Notifiers.Email;

namespace Tunney.Common.Notifiers
{
    public interface IEmailSender
    {
        IEmailer EmailNotifier { get; set; }
    }
}