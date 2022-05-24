using Microsoft.Net.Http.Headers;
using Middleware.Shared.Services;
using RabbitMQ.Client;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Logger
builder.Host.UseSerilog();

// RabbitMQ
builder.Services.AddSingleton<RabbitMQPersistentConnection>(sp => {
    var logger = sp.GetRequiredService<ILogger<RabbitMQPersistentConnection>>();
    var factory = new ConnectionFactory() { HostName = "rabbitmq" };

    return new RabbitMQPersistentConnection(logger, factory);
});

builder.Services.AddScoped<RabbitMQService>();

// Controllers
builder.Services.AddControllers(options => {
    options.ReturnHttpNotAcceptable = true;
}).AddNewtonsoftJson();

// HttpClient
builder.Services.AddHttpClient("Wordpress", httpClient => {
    httpClient.BaseAddress = new Uri("http://wordpress:80/wp-json"); // TO change to 8080 in prod.
    httpClient.DefaultRequestHeaders.Add(HeaderNames.Accept, "application/json");

});

builder.Services.AddHttpClient("UuidMasterApi", httpClient => {
    httpClient.BaseAddress = new Uri("http://api:5000/api");
    httpClient.DefaultRequestHeaders.Add(HeaderNames.Accept, "application/json");
});

// UuidMasterApi service.
builder.Services.AddScoped<UuidMasterApiService>();

// XML service.
builder.Services.AddScoped<XMLService>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UsePathBase("/api");

app.UseRouting();

app.UseAuthorization();

app.UseEndpoints(endpoints => {
    endpoints.MapControllers();
});



// app.Run(async (context) => {
//    await context.Response.WriteAsync("Hello from producer!");
// });

app.Run();
