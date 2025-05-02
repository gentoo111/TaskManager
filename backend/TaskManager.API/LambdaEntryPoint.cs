using Amazon.Lambda.AspNetCoreServer;
using Microsoft.AspNetCore.Hosting;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;

namespace TaskManager.API
{
    public class LambdaEntryPoint : APIGatewayProxyFunction
    {
        protected override void Init(IWebHostBuilder builder)
        {
            builder.UseStartup<Startup>();
        }

        // 重写方法添加日志
        public override Task<APIGatewayProxyResponse> FunctionHandlerAsync(APIGatewayProxyRequest request, ILambdaContext lambdaContext)
        {
            lambdaContext.Logger.LogLine($"Received request: {Newtonsoft.Json.JsonConvert.SerializeObject(request)}");
            
            try
            {
                return base.FunctionHandlerAsync(request, lambdaContext);
            }
            catch (Exception ex)
            {
                lambdaContext.Logger.LogLine($"Error processing request: {ex.Message}");
                lambdaContext.Logger.LogLine($"Stack trace: {ex.StackTrace}");
                
                // 返回错误响应
                return Task.FromResult(new APIGatewayProxyResponse
                {
                    StatusCode = 500,
                    Body = $"Internal server error: {ex.Message}",
                    Headers = new Dictionary<string, string> { { "Content-Type", "text/plain" } }
                });
            }
        }
    }
}