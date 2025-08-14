using System.Reflection;
using Barcoder;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;
using SkiaSharp;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddSingleton<FontService>();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi(options=>
{
     options.AddScalarTransformers(); // Required for extensions to work
     options.AddOperationTransformer<FileResultContentTypeOperationFilter>();
     options.AddDocumentTransformer((document, context, cancellationToken) =>
     {
         document.Servers = [];
         return Task.CompletedTask;
     });
});

var app = builder.Build();

app.MapOpenApi();

app.MapScalarApiReference(opt =>
{
    opt.WithClientButton(false);
    opt.WithDynamicBaseServerUrl(true);
    opt.WithTitle("Barcoder API Reference");
    opt.Favicon = "/favicon.ico";
    opt.Theme = ScalarTheme.Kepler;
    opt.Layout = ScalarLayout.Classic;

});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();



app.Run();