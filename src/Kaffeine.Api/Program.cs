using Kaffeine.Api.Features.Application;
using Kaffeine.Api.Infrastructure.SecretProtection;
using Kaffeine.Api.Persistence;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddPersistence(builder.Configuration, builder.Environment.ContentRootPath);
builder.Services.AddSecretProtection(builder.Configuration, builder.Environment.ContentRootPath);
var app = builder.Build();

// Published frontend assets are generated after the backend build.
app.UseDefaultFiles();
app.UseStaticFiles();

ApplicationFeature.MapEndpoints(app);
app.Map("/api/{**path}", () => Results.NotFound());
app.MapFallbackToFile("index.html");

app.Run();
