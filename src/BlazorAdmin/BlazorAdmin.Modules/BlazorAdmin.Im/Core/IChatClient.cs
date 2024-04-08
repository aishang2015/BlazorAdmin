using BlazorAdmin.Core.Chat;

namespace BlazorAdmin.Im.Core
{
    public interface IChatClient
    {
        Task ReceiveMessage(ChatMessageReceivedModel message);

        Task ChangeMessageCount();
    }
}
