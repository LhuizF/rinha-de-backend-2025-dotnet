using Microsoft.EntityFrameworkCore;
using Rinha.Application.Interfaces;
using Rinha.Application.Services;
using Rinha.Domain.Interfaces;
using Rinha.Infra.Consumer;
using Rinha.Infra.Gateway;
using Rinha.Infra.Messaging;
using Rinha.Infra.Persistence;
using Rinha.Infra.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<RinhaDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IMessagePublisher, InMemoryQueueMessaging>();
builder.Services.AddSingleton<IPaymentRepository, PaymentRepository>();

// HttpClients
builder.Services.AddHttpClient("DefaultProcessor", client =>
{
  var defaultUrl = builder.Configuration["PaymentProcessor:DefaultUrl"];
  client.BaseAddress = new Uri(defaultUrl!);
});

builder.Services.AddHttpClient("FallbackProcessor", client =>
{
  var fallbackUrl = builder.Configuration["PaymentProcessor:FallbackUrl"];
  client.BaseAddress = new Uri(fallbackUrl!);
});

builder.Services.AddScoped<IPaymentProcessorClient, PaymentProcessorClient>();

builder.Services.AddSingleton<InMemoryQueueMessaging>();
builder.Services.AddHostedService<PaymentMessageConsumer>();

builder.Services.AddSingleton<IMessagePublisher>(provider => provider.GetRequiredService<InMemoryQueueMessaging>());

builder.Services.AddAutoMapper(typeof(PaymentService).Assembly);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
  app.MapOpenApi();
  app.UseSwagger();
  app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
