using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorAdmin.Data.Entities.Ai
{
    [Table("AI_REQUEST_RECORD")]
    [Comment("AI请求记录")]
    public class AiRequestRecord
    {
        [Comment("主键")]
        public int Id { get; set; }

        [Comment("请求时间")]
        public DateTime RequestTime { get; set; }

        [Comment("请求Token数")]
        public int RequestTokens { get; set; }

        [Comment("响应Token数")]
        public int ResponseTokens { get; set; }

        [Comment("总价")]
        public decimal TotalPrice { get; set; }

        [Comment("请求内容")]
        public string? RequestContent { get; set; }

        [Comment("响应内容")]
        public string? ResponseContent { get; set; }
    }
}