using Microsoft.EntityFrameworkCore;

namespace BlazorAdmin.Data.Entities
{
    [Comment("聊天消息未读")]
    [Index(nameof(ChannelId))]
    [Index(nameof(ReciverId))]
    public class ChatMessageNoRead
    {
        [Comment("主键")]
        public int Id { get; set; }

        [Comment("频道")]
        public int ChannelId { get; set; }

        [Comment("接收人")]
        public int ReciverId { get; set; }

        [Comment("未读数量")]
        public int Count { get; set; }
    }
}
