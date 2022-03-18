using ImageToWebp;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;

Environment.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
//builder.Services.AddRazorPages();

var app = builder.Build();

Factory.Enable(app);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

var provider = new FileExtensionContentTypeProvider();

var exts = app.Configuration.GetSection("exts").Get<Dictionary<string, string>>();
foreach (var pair in exts)
{
    provider.Mappings[pair.Key] = pair.Value;
}
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(app.Configuration["wwwroot"]),
    ContentTypeProvider = provider
});

var pp = app.Environment.WebRootPath;

app.UseRouting();

//app.UseAuthorization();

//app.MapRazorPages();

app.Run();
