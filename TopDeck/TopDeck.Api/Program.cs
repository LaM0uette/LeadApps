using System.IO.Compression;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using TopDeck.Api.Data;
using TopDeck.Api.Endpoints;
using TopDeck.Api.Repositories;
using TopDeck.Api.Services;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

string[] allowedOrigins =
[
    "http://localhost:5277",
    "https://localhost:7164",
    "https://localhost:7184",
    "https://0.0.0.1",
    "https://api.topdeck.preprod.tehleadersheep.com",
    "https://api.topdeck.tehleadersheep.com",
    "https://app.topdeck.preprod.tehleadersheep.com",
    "https://app.topdeck.tehleadersheep.com",
];

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true);
}

builder.Configuration.AddEnvironmentVariables();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AppPolicy", policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// Brotli and Gzip compression
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});
builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});
builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});

// Json optimizations
builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        o.JsonSerializerOptions.DefaultIgnoreCondition = 
            System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
        o.JsonSerializerOptions.WriteIndented = false; // compact
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddOpenApi();

string? connectionString = builder.Configuration.GetConnectionString("Default");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("❌ ConnectionStrings:Default must be set via environment variable in Prod/Preprod, or via appsettings.Development.json in local dev.");
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(connectionString);
    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
});

// DI registrations
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IDeckItemRepository, DeckItemRepository>();
builder.Services.AddScoped<IDeckSuggestionRepository, DeckSuggestionRepository>();
builder.Services.AddScoped<IDeckDetailsRepository, DeckDetailsRepository>();
builder.Services.AddScoped<IVoteRepository, VoteRepository>();

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IDeckItemService, DeckItemService>();
builder.Services.AddScoped<IDeckDetailsService, DeckDetailsService>();
builder.Services.AddScoped<IVoteService, VoteService>();

WebApplication app = builder.Build();

using (IServiceScope scope = app.Services.CreateScope())
{
    ApplicationDbContext db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AppPolicy");

if (app.Environment.IsDevelopment())
{
    // Redirect root to Swagger UI so it's opened by default
    app.MapGet("/", () => Results.Redirect("/swagger"));
}

app.UseHttpsRedirection();
app.UseResponseCompression();

// map endpoints
app.MapUserEndpoints();
app.MapDeckItemEndpoints();
app.MapVoteEndpoints();
app.MapDeckDetailsEndpoints();
app.MapTagsEndpoints();

app.Run();
