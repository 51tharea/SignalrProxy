using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using SampleApplication.Hubs;
using SampleApplication.Services;
using SignalrProxy;
using SignalrProxy.Interfaces;

namespace SampleApplication
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        private IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();

            services.AddOptions<HubClientOptions>().Bind(Configuration.GetSection("HubConfig"));

            services.AddControllers();

            services.AddSingleton(typeof(IHubConnections<>), typeof(HubClients<>));
            services.AddSingleton<IUserService, UserService>();
            services.AddTransient<ISampleService, SampleService>();
           
            services.AddSwaggerGen(c => { c.SwaggerDoc("v1", new OpenApiInfo {Title = "SampleApplication", Version = "v1"}); });

            services.AddSignalR(options =>
            {
                options.MaximumReceiveMessageSize = null;

                //options.ClientTimeoutInterval = null;
                //options.KeepAliveInterval
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "SampleApplication v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseMiddleware<ChatMiddleware>();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();

                endpoints.MapHub<SampleHub>("/chat");
            });
        }
    }
}