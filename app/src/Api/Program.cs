using System.Runtime.InteropServices;
using Microsoft.EntityFrameworkCore;
using Rinha.Application.Interfaces;
using Rinha.Application.Services;
using Rinha.Domain.Interfaces;
using Rinha.Infra.Consumer;
using Rinha.Infra.Gateway;
using Rinha.Infra.Messaging;
using Rinha.Infra.Repositories;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

string socketPath = Environment.GetEnvironmentVariable("SOCKET_PATH") ?? throw new ArgumentNullException("SOCKET_PATH Error");

if (File.Exists(socketPath))
{
  File.Delete(socketPath);
}

builder.WebHost.ConfigureKestrel(options =>
{
  options.ListenUnixSocket(socketPath);
  options.ListenAnyIP(8080);
});


builder.Services.AddOpenApi();
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IMessagePublisher, InMemoryQueueMessaging>();
builder.Services.AddSingleton<IPaymentRepository, RedisPaymentRepository>();

builder.Services.AddHttpClient("DefaultProcessor", client =>
{
  var defaultUrl = Environment.GetEnvironmentVariable("PAYMENT_PROCESSOR_DEFAULT_URL");
  client.BaseAddress = new Uri(defaultUrl!);
});

builder.Services.AddHttpClient("FallbackProcessor", client =>
{
  var fallbackUrl = Environment.GetEnvironmentVariable("PAYMENT_PROCESSOR_FALLBACK_URL");
  client.BaseAddress = new Uri(fallbackUrl!);
});

builder.Services.AddScoped<IPaymentProcessorClient, PaymentProcessorClient>();

builder.Services.AddSingleton<InMemoryQueueMessaging>();
builder.Services.AddHostedService(
  provider => new PaymentMessageConsumer(
    provider.GetRequiredService<InMemoryQueueMessaging>(),
    provider,
    maxConcurrentProcesses: int.Parse(Environment.GetEnvironmentVariable("MAX_CONCURRENT_PROCESSES") ?? "10")
  )
);

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var redisConnection = Environment.GetEnvironmentVariable("REDIS_CONNECTION") ?? throw new ArgumentNullException("REDIS_CONNECTION Error");
    if (string.IsNullOrEmpty(redisConnection))
    {
        throw new InvalidOperationException("Redis connection string is not configured.");
    }
    return ConnectionMultiplexer.Connect(redisConnection);
});

builder.Services.AddSingleton<IMessagePublisher>(provider => provider.GetRequiredService<InMemoryQueueMessaging>());

builder.Services.AddAutoMapper(typeof(PaymentService).Assembly);

var app = builder.Build();

var applicationLifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();

applicationLifetime.ApplicationStarted.Register(() =>
{
  try
  {
        int permissions = Convert.ToInt32("777", 8);
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
          File.SetUnixFileMode(socketPath, (UnixFileMode)permissions);
        }
  }
  catch (Exception ex)
  {
    Console.WriteLine($"Socket permissions Failed: {ex.Message}");
  }
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
  app.MapOpenApi();
  app.UseSwagger();
  app.UseSwaggerUI();
}

// app.UseHttpsRedirection();

app.MapControllers();

app.Run();
