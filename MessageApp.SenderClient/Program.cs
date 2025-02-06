using MessageApp.SenderClient.Helpers;
using Newtonsoft.Json;
using Serilog;
using System.Text;

namespace MessageApp.SenderClient
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day) // Log to a file
                .CreateLogger();

            builder.Host.UseSerilog();

            var app = builder.Build();

            app.MapGet("/", async (ILogger<Program> logger) =>
            {
                using (var client = new HttpClient())
                {
                    while (true)
                    {
                        try
                        {
                            await Task.Delay(1000);

                            var randomText = StringHelper.GenerateRandomStringWithMaxLength(maxLength: 128);

                            var json = JsonConvert.SerializeObject(new { Text = randomText });

                            var content = new StringContent(json, Encoding.UTF8, "application/json");

                            var apiUrl = "https://localhost:7072/api/messages/sendmessage";

                            var response = await client.PostAsync(apiUrl, content);

                            if (!response.IsSuccessStatusCode)
                            {
                                throw new Exception($"POST request to /api/messages/sendmessage not succeeded. Response: {response.ToString()}");
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.LogError($"Error while sending request to /api/messages/sendmessage. Error message: {ex.ToString()}");
                        }
                    }
                }
            });

            app.Run();
        }
    }
}
