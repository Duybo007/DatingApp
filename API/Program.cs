using API.Data; // For database context and data-related operations
using API.Entities;
using API.Extensions; // For custom extension methods to simplify configuration
using API.MiddleWare; // For custom middleware (e.g., exception handling)
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore; // For database operations like migrations

var builder = WebApplication.CreateBuilder(args); // Creates a builder to configure the application

// Add services to the container.

// Adds application-specific services such as database context, repositories, etc.
builder.Services.AddApplicationService(builder.Configuration);

// Adds identity services for user authentication and authorization.
builder.Services.AddIdentityServices(builder.Configuration);

var app = builder.Build(); // Builds the WebApplication instance

// Configure the HTTP request pipeline.

// Adds custom exception-handling middleware to the request pipeline. Handles exceptions globally.
app.UseMiddleware<ExceptionMiddleware>();

// Configures CORS (Cross-Origin Resource Sharing) to allow requests from specific origins
app.UseCors(x => 
    x.AllowAnyHeader() // Allows all headers
     .AllowAnyMethod() // Allows all HTTP methods (GET, POST, etc.)
     .WithOrigins("http://localhost:4200", "https://localhost:4200")); // Allows requests from Angular app

// Adds authentication middleware to verify user identity. Verifies user identity based on tokens or cookies.
app.UseAuthentication();

// Adds authorization middleware to check user permissions. Enforces access permissions for authenticated users.
app.UseAuthorization();

// Maps controller routes, enabling the API endpoints
app.MapControllers();

// Create a scope for resolving scoped services
using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider; // Gets the service provider for dependency resolution

try
{
    var context = services.GetRequiredService<DataContext>(); // Resolves the database context
    var userManager = services.GetRequiredService<UserManager<AppUser>>();
    var roleManager = services.GetRequiredService<RoleManager<AppRole>>();
    await context.Database.MigrateAsync(); // Applies any pending migrations to the database
    await Seed.SeedUsers(userManager, roleManager); // Seeds initial user data into the database
}
catch (Exception ex)
{
    // Logs any exceptions that occur during migration or seeding
    var logger = services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occurred during migration");
}

// Runs the application and starts the HTTP server
app.Run();
