using GrpcCrudDemo.Services;
using GrpcCrudDemo.Data;

using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();

builder.Services.AddDbContext<AppDbContext>(opts =>
    opts.UseInMemoryDatabase("persons"));

builder.Services.AddScoped<PersonService>();

var app = builder.Build();  

// Configure the HTTP request pipeline.
app.MapGrpcService<PersonService>();

if (app.Environment.IsDevelopment())
{
    app.MapGrpcReflectionService();
}

app.MapGet("/", () =>
    "Communication with gRPC endpoints must be made through a gRPC client.");

app.Run();