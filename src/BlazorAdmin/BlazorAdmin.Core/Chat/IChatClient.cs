namespace BlazorAdmin.Core.Chat
{
    public interface IChatClient
    {
        Task ReceiveMessage(ChatMessageReceivedModel message);

        Task ChangeMessageCount();
    }
}
