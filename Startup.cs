using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using IdentityModel;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using Base64Url = Jose.Base64Url;

namespace NHS.Login.Dotnet.Core.Sample
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
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddAuthentication(options =>
                {
                    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                })
                .AddCookie()
                .AddOpenIdConnect(options =>
                {
                    options.ClientId = "YOUR-CLIENT-ID";
                    options.Authority = "https://auth.sandpit.signin.nhs.uk/";
                    options.ResponseType = "code";
                    options.ResponseMode = "form_post";
                    options.CallbackPath = "/home";
                    options.SaveTokens = true;
                    options.Events = new OpenIdConnectEvents
                    {
                        OnRedirectToIdentityProvider = context =>
                        {
                            if (context.ProtocolMessage.RequestType == OpenIdConnectRequestType.Authentication)
                            {
                                var codeVerifier = CryptoRandom.CreateUniqueId(32);
    
                                // store codeVerifier for later use
                                context.Properties.Items.Add("code_verifier", codeVerifier);
    
                                // create code_challenge
                                string codeChallenge;
                                using (var sha256 = SHA256.Create())
                                {
                                    var challengeBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(codeVerifier));
                                    codeChallenge = Base64Url.Encode(challengeBytes);
                                }
    
                                context.ProtocolMessage.Parameters.Add("code_challenge", codeChallenge);
                                context.ProtocolMessage.Parameters.Add("code_challenge_method", "S256");
                                context.ProtocolMessage.Parameters.Add("vtr", "[\"P0.Cp.Cd\", \"P0.Cp.Ck\", \"P0.Cm\"]");                           
                            }
                            
                           
                            return Task.CompletedTask;
                        },

                        OnAuthorizationCodeReceived = (context) =>
                        {
                            if (context.TokenEndpointRequest?.GrantType == OpenIdConnectGrantTypes.AuthorizationCode)
                            {
                                if (context.Properties.Items.TryGetValue("code_verifier", out var codeVerifier))
                                {
                                    context.TokenEndpointRequest.Parameters.Add("code_verifier", codeVerifier);
                                }
                                
                                context.TokenEndpointRequest.ClientAssertionType =
                                    "urn:ietf:params:oauth:client-assertion-type:jwt-bearer";
                                context.TokenEndpointRequest.ClientAssertion = TokenHelper.CreateClientAuthJwt();

                            }
                            
                            return Task.CompletedTask;
                        }
                    };
                });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseAuthentication();

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}