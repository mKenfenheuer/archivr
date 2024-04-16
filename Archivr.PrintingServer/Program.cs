using Archivr.PrintingServer.Auth;
using Microsoft.OpenApi.Models;

namespace Archivr.PrintingServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.AddSecurityDefinition(StaticTokenAuthSchemeConstants.TokenAuthScheme, new OpenApiSecurityScheme
                {
                    Description = @"Authorization header using the Bearer scheme.
                      Enter 'Bearer' [space] and then your token in the text input below.
                      Example: 'Bearer 12345abcdef'",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = StaticTokenAuthSchemeConstants.TokenAuthScheme
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type=ReferenceType.SecurityScheme,
                                Id="Bearer"
                            }
                        },
                        new string[]{
                        "Bearer"}
                    }
                });
            });

            builder.Services.AddAuthentication(
                options => options.DefaultScheme = StaticTokenAuthSchemeConstants.TokenAuthScheme)
                    .AddScheme<StaticTokenAuthSchemeOptions, StaticTokenAuthHandler>(
                        StaticTokenAuthSchemeConstants.TokenAuthScheme, options => { options.Configuration = builder.Configuration.GetRequiredSection("Security"); });



            var app = builder.Build();

            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseHttpsRedirection();

            app.UseAuthentication();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}