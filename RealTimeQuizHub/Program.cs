using RealTimeQuizHub.Services;
using RealTimeQuizHub.Services.Interfaces;

namespace RealTimeQuizHub
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);


            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddSingleton<IQuestionService>(sp =>
            {
                var env = sp.GetRequiredService<IWebHostEnvironment>();
                var filePath = Path.Combine(env.ContentRootPath, "Data", "questions.json");
                return new JsonQuestionService(filePath);
            });
            builder.Services.AddSingleton<IQuizSessionService, QuizSessionService>();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
