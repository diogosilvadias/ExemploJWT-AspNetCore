using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExemploJWT.Database;
using ExemploJWT.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ExemploJWT
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddScoped<DbHelper>(db => new DbHelper());
            services.AddScoped<UserDAO>();

            var signingConfiguration = new SigningConfigurations();
            services.AddSingleton(signingConfiguration);

            var tokenConfiguration = new TokenConfigurations();
            new ConfigureFromConfigurationOptions<TokenConfigurations>(
                Configuration.GetSection("TokenConfigurations")
            ).Configure(tokenConfiguration);
            services.AddSingleton(tokenConfiguration);

            services.AddAuthentication(authOpts =>
            {
                authOpts.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                authOpts.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(bearerOpts =>
            {
                var paramsValidation = bearerOpts.TokenValidationParameters;
                paramsValidation.IssuerSigningKey = signingConfiguration.Key;
                paramsValidation.ValidAudience = tokenConfiguration.Audience;
                paramsValidation.ValidIssuer = tokenConfiguration.Issuer;

                // Valida a assinatura de um token recebido
                paramsValidation.ValidateIssuerSigningKey = true;

                // Verifica se um token recebido ainda é válido
                paramsValidation.ValidateLifetime = true;

                // Tempo de tolerância para a expiração de um token (utilizado
                // caso haja problemas de sincronismo de horário entre diferentes
                // computadores envolvidos no processo de comunicação)
                paramsValidation.ClockSkew = TimeSpan.Zero;
            });

            // services.AddAuthorization(auth =>
            // {
            //     auth.AddPolicy("Bearer", new AuthorizationPolicyBuilder()
            //         .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
            //         .RequireAuthenticatedUser().Build());
            // });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
