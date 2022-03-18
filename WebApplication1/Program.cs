using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider("d:\\aaa"),    
    OnPrepareResponse = ctx =>
    {
        var body = ctx.Context.Response.Body;
        // Add CSP for index.html
        if (ctx.File.Name == "index.html")
        {
            ctx.Context.Response.Headers.Append(
               "Content-Security-Policy", "default-src 'self'" // etc
            );
        }
    }
});
app.UseAuthorization();

app.MapControllers();

app.Run();
