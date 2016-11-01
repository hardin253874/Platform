using EDC.ReadiNow.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReadiNow.Integration.Sms
{
    public interface ISmsProvider
    {
        Task<IEnumerable<SendResponse>> SendMessage(long notifierId, string accountSid, string authToken, string sendingNumber, string message,  IEnumerable<string> numbers);

        void RegisterUrlForIncomingSms(string accountSid, string authToken, string receivingNumber, long notifierId);

        void DeregisterUrlForIncomingSms(string accountSid, string authToken, string receivingNumber);

    }

    public struct SendResponse
    {
        public string Number;
        public bool Success;
        public string Message;
        public string MessageSid;
    }
}
