using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using RealTimeQuizHub.Services;
using RealTimeQuizHub.Services.Interfaces;
using RealTimeQuizHub.Models;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using RealTimeQuizHub.Repository.Interfaces;
using RealTimeQuizHub.Repository;

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
            builder.Services.AddSwaggerGen();

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

            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();
            builder.Services.AddScoped<IAnswerRepository, AnswerRepository>();

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
                      ValidateIssuerSigningKey = true
                  };
              });
            builder.Services.AddAuthorization(options => {
                options.AddPolicy("AdminOnly", p => p.RequireClaim("isAdmin", "True", "true"));
            });

            builder.Services.AddCors(options => {
                options.AddPolicy("AllowAll", policy =>
                    policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
            });


            var app = builder.Build();



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
