using System.IO.Compression;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using TopDeck.Api.Data;
using TopDeck.Api.Endpoints;
using TopDeck.Api.Repositories;
using TopDeck.Api.Repositories.Interfaces;
using TopDeck.Api.Services;
using TopDeck.Api.Services.Interfaces;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

string[] allowedOrigins =
[
    "http://localhost:5277",
    "https://localhost:7164",
    "https://localhost:7184",
    "https://0.0.0.1"
];

builder.Services.AddCors(options =>
{
    options.AddPolicy("AppPolicy", policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
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

string connectionString = builder.Configuration.GetConnectionString("Default") ?? throw new InvalidOperationException("ConnectionStrings: Default is not configured in appsettings.json or environment.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(connectionString);
});

// DI registrations
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IDeckRepository, DeckRepository>();
builder.Services.AddScoped<IDeckSuggestionRepository, DeckSuggestionRepository>();
builder.Services.AddScoped<IDeckLikeRepository, DeckLikeRepository>();
builder.Services.AddScoped<IDeckSuggestionLikeRepository, DeckSuggestionLikeRepository>();
builder.Services.AddScoped<IDeckDislikeRepository, DeckDislikeRepository>();
builder.Services.AddScoped<IDeckSuggestionDislikeRepository, DeckSuggestionDislikeRepository>();

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IDeckService, DeckService>();
builder.Services.AddScoped<IDeckSuggestionService, DeckSuggestionService>();
builder.Services.AddScoped<IDeckLikeService, DeckLikeService>();
builder.Services.AddScoped<IDeckSuggestionLikeService, DeckSuggestionLikeService>();
builder.Services.AddScoped<IDeckDislikeService, DeckDislikeService>();
builder.Services.AddScoped<IDeckSuggestionDislikeService, DeckSuggestionDislikeService>();

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AppPolicy");

// Redirect root to Swagger UI so it's opened by default
app.MapGet("/", () => Results.Redirect("/swagger"));

app.UseHttpsRedirection();
app.UseResponseCompression();

// map endpoints
app.MapUsersEndpoints();
app.MapDecksEndpoints();
app.MapDeckSuggestionsEndpoints();
app.MapDeckLikesEndpoints();
app.MapDeckSuggestionLikesEndpoints();
app.MapDeckDislikesEndpoints();
app.MapDeckSuggestionDislikesEndpoints();

app.Run();
