using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorAdmin.Core.Chat
{
    public interface IChatClient
    {
        Task ReceiveMessage(ChatMessageReceivedModel message);

        Task ChangeMessageCount();
    }
}
