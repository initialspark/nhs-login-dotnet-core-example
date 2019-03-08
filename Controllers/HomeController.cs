using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NHS.Login.Dotnet.Core.Sample.Models;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;

namespace NHS.Login.Dotnet.Core.Sample.Controllers
{
    
    public class HomeController : Controller
    {
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            
            var client = new HttpClient();

            var disco = await client.GetDiscoveryDocumentAsync("https://auth.sandpit.signin.nhs.uk/");

            var response = await client.GetUserInfoAsync(new UserInfoRequest
            {
                Address = "https://auth.sandpit.signin.nhs.uk/userinfo",
                Token = accessToken
            });
            
            return View(response);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
