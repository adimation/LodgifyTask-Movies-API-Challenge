using ApiApplication.ApiClient;
using ApiApplication.Configurations;
using ApiApplication.Database;
using ApiApplication.Database.Repositories;
using ApiApplication.Database.Repositories.Abstractions;
using ApiApplication.DTOs.TicketDTOs;
using ApiApplication.Mappers;
using ApiApplication.Middlewares;
using ApiApplication.Services;
using ApiApplication.Services.Abstractions;
using ApiApplication.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace ApiApplication
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
            // Register repositories
            services.AddTransient<IShowtimesRepository, ShowtimesRepository>();
            services.AddTransient<ITicketsRepository, TicketsRepository>();
            services.AddTransient<IAuditoriumsRepository, AuditoriumsRepository>();

            // Register services
            services.AddTransient<IShowtimeService, ShowtimeService>();
            services.AddTransient<ITicketService, TicketService>();
            services.AddScoped<IApiClientGrpc, ApiClientGrpc>();

            // Register configurations
            services.Configure<MoviesApiConfig>(Configuration.GetSection("MoviesApiConfig"));
            services.Configure<GeneralConfig>(Configuration.GetSection("GeneralConfig"));

            // Register validators
            services.AddTransient<IValidator<ReserveTicketRequestDTO>, ReserveTicketRequestDTOValidator>();
            
            // Register Redis cache
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = Configuration.GetConnectionString("RedisConnection");
                options.ConfigurationOptions = new StackExchange.Redis.ConfigurationOptions()
                {
                    AbortOnConnectFail = true,
                    EndPoints = { options.Configuration }
                };
            });

            services.AddDbContext<CinemaContext>(options =>
            {
                options.UseInMemoryDatabase("CinemaDb")
                    .EnableSensitiveDataLogging()
                    .ConfigureWarnings(b => b.Ignore(InMemoryEventId.TransactionIgnoredWarning));
            });

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo() { Version = "v1", Title = "Cinema API" });
            });

            services.AddControllers();

            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });

            // Register AutoMapper
            services.AddAutoMapper(typeof(MappingProfile).Assembly);

            // Register Mini Profiler in degub mode only
#if DEBUG
            services.AddMiniProfiler(options =>
            {
                options.RouteBasePath = "/profiler";
                options.SqlFormatter = new StackExchange.Profiling.SqlFormatters.SqlServerFormatter();
            }).AddEntityFramework();
#endif

            services.AddHttpClient();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //if (env.IsDevelopment())
            //{
            //    app.UseDeveloperExceptionPage();
            //}

            // Custom Exception handler middleware
            app.UseMiddleware<CustomExceptionHandlerMiddleware>();

            //Miniprofiler integration in debug mode
#if DEBUG
            app.UseMiniProfiler();
#endif
            // Swagger integration
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Swagger API V1");
            });

            app.UseHttpsRedirection();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            // Add the logging middleware
            app.UseMiddleware<LogExecutionTimeMiddleware>();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            SampleData.Initialize(app);
        }      
    }
}
