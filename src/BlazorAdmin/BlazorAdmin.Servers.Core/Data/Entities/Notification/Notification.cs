using System.ComponentModel.DataAnnotations;

namespace BlazorAdmin.Servers.Core.Data.Entities.Notification
{
    /// <summary>
    /// 系统通知实体
    /// </summary>
    public class Notification
    {
        /// <summary>
        /// 主键Id
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// 通知标题
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// 通知内容
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// 发送者Id
        /// </summary>
        public int SenderId { get; set; }

        /// <summary>
        /// 发送时间
        /// </summary>
        public DateTime SendTime { get; set; }

        /// <summary>
        /// 通知类型（1:系统通知 2:个人通知）
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 通知状态（0:草稿 1:已发送）
        /// </summary>
        public int Status { get; set; }
    }
}