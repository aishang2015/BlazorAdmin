using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorAdmin.Data.Entities.Ai
{
    [Table("AI_CONFIG")]
    [Comment("AI配置")]
    public class AiConfig
    {
        [Comment("主键")]
        public int Id { get; set; }

        [Comment("接口地址")]
        public string? Endpoint { get; set; }

        [Comment("API密钥")]
        public string? ApiKey { get; set; }

        [Comment("模型名称")]
        public string? ModelName { get; set; }

        [Comment("输入单价(每百万Token)")]
        public decimal? InputPricePerToken { get; set; }

        [Comment("输出单价(每百万Token)")]
        public decimal? OutputPricePerToken { get; set; }

        [Comment("上下文长度")]
        public int ContextLength { get; set; }

        [Comment("配置描述")]
        public string? Description { get; set; }

        [Comment("编码")]
        public string? Code { get; set; }

        [Comment("是否启用")]
        public bool IsEnabled { get; set; }

        [Comment("是否删除")]
        public bool IsDeleted { get; set; }
    }
}