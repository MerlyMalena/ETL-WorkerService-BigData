using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Agrega soporte para los Controllers (como nuestro SocialCommentsController)
builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();

app.Run();
