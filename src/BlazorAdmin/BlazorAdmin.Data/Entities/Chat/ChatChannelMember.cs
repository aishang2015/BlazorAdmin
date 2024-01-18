using Microsoft.EntityFrameworkCore;

namespace BlazorAdmin.Data.Entities.Chat
{
    [Comment("聊天成员")]
    [Index(nameof(ChatChannelId))]
    [Index(nameof(MemberId))]
    public class ChatChannelMember
    {
        [Comment("主键")]
        public int Id { get; set; }

        [Comment("聊天频道Id")]
        public int ChatChannelId { get; set; }

        [Comment("成员Id")]
        public int MemberId { get; set; }

        [Comment("此成员未读消息数量")]
        public int NotReadCount { get; set; }
    }
}
