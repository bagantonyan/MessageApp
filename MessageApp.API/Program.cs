using FluentValidation;
using FluentValidation.AspNetCore;
using MessageApp.API.Handlers;
using MessageApp.API.Hubs;
using MessageApp.API.Mappings;
using MessageApp.API.ModelValidators.Messages;
using MessageApp.DAL.Helpers;
using MessageApp.DAL.Repositories;
using MessageApp.DAL.Repositories.Interfaces;
using Serilog;

namespace MessageApp.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            builder.Host.UseSerilog();

            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
            builder.Services.AddProblemDetails();

            builder.Services.AddScoped<IMessageRepository, MessageRepository>();
            builder.Services.AddSingleton<DatabaseInitializer>();

            builder.Services.AddAutoMapper(config =>
            {
                config.AddProfile<APIMappingProfile>();
            });

            builder.Services.AddValidatorsFromAssembly(typeof(SendMessageRequestModelValidator).Assembly);
            builder.Services.AddFluentValidationAutoValidation();
            builder.Services.AddFluentValidationClientsideAdapters();

            builder.Services.AddSignalR();

            builder.Services.AddControllers();

            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader());
            });

            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            app.UseExceptionHandler();

            var logger = app.Services.GetRequiredService<ILogger<Program>>();

            try
            {
                logger.LogInformation("Starting database initialization...");

                var dbInitializer = app.Services.GetRequiredService<DatabaseInitializer>();
                await dbInitializer.EnsureDatabaseAndTableCreatedAsync();

                logger.LogInformation("Database initialization completed.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred during database initialization.");
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.UseCors("CorsPolicy");

            app.MapHub<MessageHub>("/messageHub");

            app.Use(async (context, next) =>
            {
                logger.LogInformation($"Request: {context.Request.Method} {context.Request.Path}");
                await next.Invoke();
            });

            app.Run();
        }
    }
}