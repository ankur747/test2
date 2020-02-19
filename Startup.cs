using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using Hangfire;
using Hangfire.PostgreSql;

namespace Test2
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
            services.AddControllers();


        var connectionStringTestDB = Configuration["ConnectionStringTestDB"];
        var connectionString = Configuration["ConnectionString"];

        //services.AddDbContext<testContext>(options => options.UseNpgsql(connectionStringTestDB));
        services.AddDbContext<BankMasterLocalContext>(options => options.UseNpgsql(connectionString));

            services.AddHangfire(x => x.UsePostgreSqlStorage(connectionString));

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseHangfireDashboard(); //Will be available under http://localhost:5000/hangfire"
            app.UseHangfireServer();

            //Recurring
            RecurringJob.AddOrUpdate<BankServiceJobs>((job) => job.UpdateAllBranchDetails(), Cron.Yearly(3));

            //Continuation
            var id = BackgroundJob.Enqueue(() => Console.WriteLine("Hello, "));
            BackgroundJob.ContinueJobWith(id, () => Console.WriteLine("world!"));

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
