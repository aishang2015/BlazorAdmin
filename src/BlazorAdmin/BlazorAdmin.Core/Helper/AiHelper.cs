using BlazorAdmin.Data;
using BlazorAdmin.Data.Entities.Ai;
using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;
using OpenAI.Chat;
using System.Diagnostics;

namespace BlazorAdmin.Core.Helper
{
    public class AiHelper
    {
        private readonly IDbContextFactory<BlazorAdminDbContext> _dbContextFactory;

        public AiHelper(IDbContextFactory<BlazorAdminDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<string?> ChatToAi(string configCode, string prompt,
            Dictionary<string, string> parameterDictionary)
        {
            using var context = await _dbContextFactory.CreateDbContextAsync();
            var config = context.AiConfigs.FirstOrDefault(c => c.Code == configCode);
            if (config == null)
            {
                throw new Exception("Config not found");
            }

            var kernelBuilder = Kernel.CreateBuilder();
            var kernel = kernelBuilder.AddOpenAIChatCompletion(
                modelId: config.ModelName,
                apiKey: config.ApiKey,
                httpClient: new HttpClient(new OpenAICompatibleHandler(config.Endpoint))
                {
                    Timeout = TimeSpan.FromMinutes(5) // 设置超时时间为5分钟
                }).Build();

            var parameters = new KernelArguments();
            foreach (var (key, value) in parameterDictionary)
            {
                parameters[key] = value;
            }

            var stopWatch = new Stopwatch();
            stopWatch.Start();

            var promptResult = await kernel.InvokePromptAsync(prompt, parameters);

            stopWatch.Stop();

            var responseText = promptResult.GetValue<string>();

            var usage = promptResult.Metadata?["Usage"] as ChatTokenUsage;

            decimal totalPrice = 0;
            if (config.InputPricePerToken != null && config.InputPricePerToken > 0)
            {
                totalPrice += config.InputPricePerToken.Value * usage.InputTokenCount;
            }
            if (config.OutputPricePerToken != null && config.OutputPricePerToken > 0)
            {
                totalPrice += config.OutputPricePerToken.Value * usage.OutputTokenCount;
            }

            context.AiRequestRecords.Add(new AiRequestRecord
            {
                RequestTime = DateTime.Now,
                ElapsedMilliseconds = (int)stopWatch.ElapsedMilliseconds,
                RequestTokens = usage.InputTokenCount,
                ResponseTokens = usage.OutputTokenCount,
                RequestContent = prompt,
                ResponseContent = responseText,
                TotalPrice = totalPrice
            });
            context.SaveChanges();
            return responseText;
        }

        public async Task<(bool IsSuccess, string? ErrorMessage)> TestAiConfig(string modelName, string apiKey, string endpoint)
        {
            try
            {
                var kernelBuilder = Kernel.CreateBuilder();
                var kernel = kernelBuilder.AddOpenAIChatCompletion(
                    modelId: modelName,
                    apiKey: apiKey,
                    httpClient: new HttpClient(new OpenAICompatibleHandler(endpoint))
                    {
                        Timeout = TimeSpan.FromMinutes(5) // 设置超时时间为5分钟
                    }).Build();

                var parameters = new KernelArguments();

                var stopWatch = new Stopwatch();
                stopWatch.Start();

                var promptResult = await kernel.InvokePromptAsync("请回复hello，不需要别的内容", parameters);

                stopWatch.Stop();

                var responseText = promptResult.GetValue<string>();

                var usage = promptResult.Metadata?["Usage"] as ChatTokenUsage;

                using var context = await _dbContextFactory.CreateDbContextAsync();
                context.AiRequestRecords.Add(new AiRequestRecord
                {
                    RequestTime = DateTime.Now,
                    ElapsedMilliseconds = (int)stopWatch.ElapsedMilliseconds,
                    RequestTokens = usage.InputTokenCount,
                    ResponseTokens = usage.OutputTokenCount,
                    RequestContent = "请回复hello，不需要别的内容",
                    ResponseContent = responseText,
                });
                context.SaveChanges();
                return (true, $"AI 配置测试成功");
            }
            catch (Exception ex)
            {
                return (false, $"AI 配置测试失败: {ex.Message}");
            }
        }
    }

    public class OpenAICompatibleHandler : HttpClientHandler
    {
        private readonly string modelUrl;
        private static readonly string[] sourceArray = ["api.openai.com", "openai.azure.com"];

        public OpenAICompatibleHandler(string modelUrl)
        {
            // 确保modelUrl不是null或空
            if (string.IsNullOrWhiteSpace(modelUrl))
                throw new ArgumentException("模型URL不能为空或空白。", nameof(modelUrl));

            this.modelUrl = modelUrl;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // 检查请求是否针对OpenAI或Azure OpenAI服务
            if (request.RequestUri != null &&
                (sourceArray.Contains(request.RequestUri.Host)))
            {
                // 修改请求URI,以包含模型URL
                request.RequestUri = new Uri(modelUrl + request.RequestUri.PathAndQuery);
            }
            // 调用基类方法实际发送HTTP请求
            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
    }

}
