using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorAdmin.Data.Entities.Ai
{
    [Table("AI_PROMPT")]
    [Comment("AI提示词配置")]
    public class AiPrompt
    {
        [Comment("主键")]
        public int Id { get; set; }

        [Comment("提示词名称")]
        public string? PromptName { get; set; }

        [Comment("提示词内容")]
        public string? PromptContent { get; set; }
    }
}
