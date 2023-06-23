using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PMSAPI.DataAccess.Data;
using PMSAPI.DataAccess.Repository.IRepository;
using PMSAPI.DataAccess.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
[assembly: FunctionsStartup(typeof(PMSAPI.Startup))]

namespace PMSAPI
{
    public class Startup: FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            string connection = Environment.GetEnvironmentVariable("DefaultConnection");
            builder.Services.AddDbContext<AppDbContext>(options => options.UseMySql(
                connection, ServerVersion.AutoDetect(connection)));

            builder.Services.AddScoped<IRepository, Repository>();

        }
    }
}
