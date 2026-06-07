using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RealTimeQuizHub.Hubs;
using RealTimeQuizHub.Models;
using RealTimeQuizHub.Repository;
using RealTimeQuizHub.Repository.Interfaces;
using RealTimeQuizHub.Services;
using RealTimeQuizHub.Services.Interfaces;
using System.Text;

namespace RealTimeQuizHub
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var jwtSettings = new JwtSettings();
            builder.Configuration.Bind("JwtSettings", jwtSettings);
            builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                // 1. Описуємо схему авторизації (як Swagger має передавати токен)
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Введіть токен у форматі: Bearer {ваш_токен}"
                });

                // 2. Додаємо глобальну вимогу авторизації для захищених ендпоінтів
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>() // Порожній масив означає, що специфічні scopes не вимагаються
                    }
                });
            });

            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddSingleton<IJsonQuestionService>(sp =>
            {
                var env = sp.GetRequiredService<IWebHostEnvironment>();
                var filePath = Path.Combine(env.ContentRootPath, "Data", "questions.json");
                return new JsonQuestionService(filePath);
            });

            
            builder.Services.AddSingleton<IJwtGenerator, JwtGenerator>();

            builder.Services.AddScoped<IQuizSessionService, QuizSessionService>();

            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IAnswerService, AnswerService>();
            builder.Services.AddScoped<IQuestionService, QuestionService>();
            builder.Services.AddScoped<IQuizService, QuizService>();
            builder.Services.AddScoped<IScoreService, ScoreService>();
            builder.Services.AddScoped<IBadgeService, BadgeService>();
            builder.Services.AddScoped<ILeaderboardService, LeaderboardService>();

            // Per-quiz leaderboards are kept in memory only (reset on restart);
            // a singleton keeps them alive for the lifetime of the process.
            builder.Services.AddSingleton<IQuizLeaderboardStore, QuizLeaderboardStore>();

            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();
            builder.Services.AddScoped<IAnswerRepository, AnswerRepository>();
            builder.Services.AddScoped<IQuizRepository, QuizRepository>();
            builder.Services.AddScoped<IScoreRepository, ScoreRepository>();
            builder.Services.AddScoped<IBadgeRepository, BadgeRepository>();

            builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();


            builder.Services
              .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
              .AddJwtBearer(opts => {
                  opts.TokenValidationParameters = new TokenValidationParameters
                  {
                      ValidateIssuer = true,
                      ValidIssuer = jwtSettings.Issuer,
                      ValidateAudience = true,
                      ValidAudience = jwtSettings.Audience,
                      ValidateLifetime = true,
                      IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
                      ValidateIssuerSigningKey = true,
                      ClockSkew = TimeSpan.Zero
                  };

                  // SignalR sends the JWT via the "access_token" query string
                  // parameter (WebSockets/SSE can't set Authorization headers).
                  opts.Events = new JwtBearerEvents
                  {
                      OnMessageReceived = context =>
                      {
                          var accessToken = context.Request.Query["access_token"];
                          var path = context.HttpContext.Request.Path;
                          if (!string.IsNullOrEmpty(accessToken) &&
                              path.StartsWithSegments("/quizHub"))
                          {
                              context.Token = accessToken;
                          }
                          return Task.CompletedTask;
                      }
                  };
              });
            builder.Services.AddAuthorization(options => {
                options.AddPolicy("AdminOnly", p => p.RequireClaim("isAdmin", "True", "true"));
            });

            builder.Services.AddCors(options => {
                options.AddPolicy("AllowAll", policy =>
                    policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
            });


            builder.Services.AddSignalR();


            var app = builder.Build();

            // One-off sample-data seeding: `dotnet run -- seed`. Seeds quizzes then exits.
            if (args.Contains("seed", StringComparer.OrdinalIgnoreCase))
            {
                using var scope = app.Services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var added = Data.QuizSeeder.Seed(db);
                Console.WriteLine($"Seeded {added} new quiz(zes).");
                return;
            }


            app.MapHub<QuizHub>("/quizHub");

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.UseHttpsRedirection();

            app.UseCors("AllowAll");

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
