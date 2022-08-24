﻿
using DateApp.API.Helpers;
using DateApp.Data.Abstract;
using DateApp.Data.Concrete;
using DateApp.Data.Interfaces;
using DateApp.Data.Services;
using DateApp.Entity.DataContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DateApp.API.Extensions
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services,IConfiguration config)

        {
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddAutoMapper(typeof(AutoMapperProfiles).Assembly);
            services.AddDbContext<DataContextModel>(options =>
            {
                options.UseSqlServer(config.GetConnectionString("DateAppDb"));
            });
            return services;
        }

    }
}
