using Amazon.SQS;
using Inventory.API.Data;
using Microsoft.EntityFrameworkCore;
using Inventory.OutboxProcessor;

var builder = Host.CreateApplicationBuilder(args);

// Configurar Base de Datos
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configurar Cliente AWS SQS para LocalStack
var awsSection = builder.Configuration.GetSection("AWS");
var sqsConfig = new AmazonSQSConfig
{
    ServiceURL = awsSection["ServiceURL"], // Apunta a localhost:4566
    AuthenticationRegion = awsSection["Region"]
};
var awsCredentials = new Amazon.Runtime.BasicAWSCredentials(awsSection["AccessKey"], awsSection["SecretKey"]);

// Registramos el cliente SQS como Singleton para que todo el proyecto pueda usarlo
builder.Services.AddSingleton<IAmazonSQS>(new AmazonSQSClient(awsCredentials, sqsConfig));

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();