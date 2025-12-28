using Game.Application.UseCases;
using Game.Domain.Content;
using Game.Domain.Services;
using Game.Domain.Run;
using Game.Infrastructure.Baking;
using Game.Infrastructure.Content.CMS;
using Game.Infrastructure.Random;
using Game.UI.Components;
using Game.UI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Configure content path - point to ConsoleRunner content directory
var contentPath = Path.Combine(
    builder.Environment.ContentRootPath,
    "..", "Game.ConsoleRunner", "Content", "Json"
);
var absoluteContentPath = Path.GetFullPath(contentPath);

// Fallback: try alternative paths if first doesn't exist
if (!Directory.Exists(absoluteContentPath))
{
    // Try relative to base directory
    var altPath1 = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory,
        "..", "..", "..", "..", "Game.ConsoleRunner", "Content", "Json"
    );
    var altAbsolutePath1 = Path.GetFullPath(altPath1);
    if (Directory.Exists(altAbsolutePath1))
    {
        absoluteContentPath = altAbsolutePath1;
    }
    else
    {
        // Try from solution root
        var solutionPath = Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, "..", ".."));
        var solutionContentPath = Path.Combine(solutionPath, "Game.ConsoleRunner", "Content", "Json");
        if (Directory.Exists(solutionContentPath))
        {
            absoluteContentPath = solutionContentPath;
        }
    }
}

// Log the content path for debugging
Console.WriteLine($"Content path: {absoluteContentPath}");
Console.WriteLine($"Content path exists: {Directory.Exists(absoluteContentPath)}");
if (Directory.Exists(absoluteContentPath))
{
    var deckFile = Path.Combine(absoluteContentPath, "Decks", "standard_deck.json");
    Console.WriteLine($"Deck file exists: {File.Exists(deckFile)}");
}

// Register infrastructure services
builder.Services.AddSingleton<IContentManager>(sp => 
    new JsonContentManager(absoluteContentPath));
builder.Services.AddSingleton<IDeckContentLoader, DeckContentLoaderAdapter>();

// Register domain services
builder.Services.AddScoped<IRunService, RunService>();

// Register application use cases
builder.Services.AddScoped<StartRunUseCase>();

// Register UI services (scoped - one game per user)
builder.Services.AddScoped<IGameSessionService, GameSessionService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();