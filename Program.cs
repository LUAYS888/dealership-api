using System.Text;
using Dealership.Api.Auth;
using Dealership.Api.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using FluentValidation;
using FluentValidation.AspNetCore;
using Dealership.Api.Middleware;

namespace Dealership.Api
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();   // Required for Swagger
            //builder.Services.AddSwaggerGen();             // Register Swagger

            builder.Services.AddFluentValidationAutoValidation();         //For input validation.
            builder.Services.AddValidatorsFromAssemblyContaining<Program>(); // scans assembly

            builder.Services.AddSwaggerGen(c =>                                        
            {
                // Show "Authorize" button and send JWT with requests
                c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                    Description = "Enter: Bearer {your JWT}"
                });

                c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
            });











            // DbContext (SQLite)
            var cs = builder.Configuration.GetConnectionString("Default")
                     ?? "Data Source=dealership.db";
            builder.Services.AddDbContext<AppDbContext>(opt =>
                opt.UseSqlite(cs));


            // Auth services
            builder.Services.AddSingleton<PasswordHasher>();
            builder.Services.AddSingleton<JwtTokenService>();
            builder.Services.AddScoped<OtpService>();

            // JWT authentication
            var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!);
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(o =>
                {
                    o.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = builder.Configuration["Jwt:Issuer"],
                        ValidAudience = builder.Configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ClockSkew = TimeSpan.Zero
                    };
                });

            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();

            var app = builder.Build();
            app.UseMiddleware<ErrorHandlingMiddleware>();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();  // Swagger UI at /swagger
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            //app.Run();

            //// Apply migrations & seed on startup
            //using (var scope = app.Services.CreateScope())
            //{
            //    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            //    await db.Database.MigrateAsync();
            //    await Seed.RunAsync(db);
            //}

            // Migrate + seed
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                await db.Database.MigrateAsync();

                // hash the seeded admin's password if still placeholder
                if (!db.Users.Any(u => u.Email == "luay@local.test"))
                {
                    var hasher = scope.ServiceProvider.GetRequiredService<PasswordHasher>();
                    db.Users.Add(new Dealership.Api.Domain.User
                    {
                        Email = "luay@local.test",
                        PasswordHash = hasher.Hash("Admin@12345"),
                        Role = Dealership.Api.Domain.Role.Admin
                    });
                    await db.SaveChangesAsync();
                }

                await Seed.RunAsync(db); // keeps vehicles seeding
            }

            await app.RunAsync();
        }
    }
}
