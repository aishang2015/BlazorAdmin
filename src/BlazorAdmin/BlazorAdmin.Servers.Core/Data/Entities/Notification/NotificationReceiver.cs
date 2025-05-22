using System.ComponentModel.DataAnnotations;

namespace BlazorAdmin.Servers.Core.Data.Entities.Notification
{
    /// <summary>
    /// 通知接收记录实体
    /// </summary>
    public class NotificationReceiver
    {
        /// <summary>
        /// 主键Id
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// 通知Id
        /// </summary>
        public int NotificationId { get; set; }

        /// <summary>
        /// 接收者Id
        /// </summary>
        public int ReceiverId { get; set; }

        /// <summary>
        /// 是否已读
        /// </summary>
        public bool IsRead { get; set; }

        /// <summary>
        /// 阅读时间
        /// </summary>
        public DateTime? ReadTime { get; set; }
    }
}