using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using grup_gadu_api.Data;
using grup_gadu_api.Helpers;
using grup_gadu_api.Interfaces;
using grup_gadu_api.Services;
using System.Text;
using System.Reflection;
using System.IO;
using System;
using grup_gadu_api.Hubs;
using System.Threading.Tasks;
using System.Linq;

namespace grup_gadu_api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IChatRepository, ChatRepository>();
            services.AddScoped<IMessagesService, MessagesService>();
            services.AddAutoMapper(typeof(AutoMapperProfiles).Assembly);
            services.AddSignalR();
            services.AddDbContext<DataContext>(opt =>
            {
              opt.UseSqlite(Configuration.GetConnectionString("DefaultConnection"));
            });
            services.AddCors(options => options.AddPolicy("CorsPolicy", builder =>
            {
                builder
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .WithOrigins("http://localhost:3000")
                    .AllowCredentials();
            }));
            services.AddControllers();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(opt => {
                opt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["TokenKey"])),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
                opt.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var path = context.HttpContext.Request.Path;
                        if(path.StartsWithSegments("/hubs/chat"))
                        {
                            var authHeader = context.HttpContext.Request.Headers.FirstOrDefault(x => x.Key == "Authorization");
                            var tokenFromHeader = authHeader.Value.ToString().Replace("Bearer ", string.Empty);
                            var tokenFromQuery = context.HttpContext.Request.Query.FirstOrDefault(x => x.Key == "access_token");

                            context.Token = !string.IsNullOrEmpty(tokenFromHeader) ? tokenFromHeader : tokenFromQuery.Value;
                        }
                        return Task.CompletedTask;
                    }
                };
            });
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "grup_gadu_api", Version = "v1" });

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "grup_gadu_api v1"));
            }
            app.UseCors("CorsPolicy");
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<ChatHub>("/hubs/chat");
            });
        }
    }
}
