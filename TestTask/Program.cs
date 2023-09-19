using Microsoft.EntityFrameworkCore;
using TestTask.Services;
using TestTask.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddControllers();

builder.Services.AddDbContextFactory<BlobContext>(opt =>
    opt.UseSqlite("Data Source=DB.db"));

builder.Services.AddScoped<IBlobStorageService, BlobStorageService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var dataContext = services.GetRequiredService<BlobContext>();
    DbInitializer.Initialize(dataContext);
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapControllers();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();