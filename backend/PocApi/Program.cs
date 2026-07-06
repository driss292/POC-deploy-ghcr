using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

// --- 1. CONFIGURATION DES SERVICES ---

// C'EST ICI : On récupère la connexion AVANT d'ajouter le DbContext
var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING")
                    ?? builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin() // On autorise TOUT
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// --- 2. CONFIGURATION DU PIPELINE (MIDDLEWARE) ---

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Mon POC API v1");
        c.RoutePrefix = string.Empty; // Swagger à la racine
    });
}

app.UseCors("AllowAll");

// --- 3. ENDPOINTS ---

app.MapPost("/messages", async (Message msg, AppDbContext db) =>
{
    if (string.IsNullOrEmpty(msg.Content))
        return Results.BadRequest("Le contenu est vide");

    db.Messages.Add(msg);
    await db.SaveChangesAsync();

    return Results.Ok(new { message = "Bien reçu !", data = msg });
});

app.MapGet("/ping", () => Results.Ok("L'API répond bien !"));

// --- 4. AUTO-MIGRATION (Très important pour Render/Supabase) ---
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    // On attend un peu ou on gère l'erreur au cas où la DB ne soit pas prête au démarrage Docker
    try
    {
        db.Database.Migrate();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Erreur migration : {ex.Message}");
    }
}

app.Run();

// --- 5. MODÈLES & CONTEXTE ---

public class Message
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
}

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<Message> Messages => Set<Message>();
}