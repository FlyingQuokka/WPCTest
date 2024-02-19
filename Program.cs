using WPCTest.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// Register the CrimeDataService for dependency injection
builder.Services.AddSingleton<CrimeDataService>();

// Add HttpClient services
builder.Services.AddHttpClient();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment()) {
    app.UseExceptionHandler("/Error");
    app.UseStatusCodePagesWithReExecute("/Error/{0}");
} else {
    app.UseDeveloperExceptionPage();
}

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

// Custom error handling middleware for logging uncaught exceptions
app.Use(async (context, next) => {
    try
    {
        await next();
    } catch (Exception ex) {
        app.Logger.LogError(ex, "An unhandled exception has occurred.");
        context.Response.Redirect("/Error");
    }
});

app.Run();

public partial class Program {}
