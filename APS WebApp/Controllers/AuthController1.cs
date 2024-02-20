using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using System.Text;
using System;
using System.IO;
using System.Net;

namespace APS_WebApp.Controllers
{
    public class AuthController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly string returnUri = "https://localhost:8080/";
        public AuthController(IHttpClientFactory clientFactory , IHttpContextAccessor contextAccessor)
        {
            _clientFactory = clientFactory;
            _contextAccessor = contextAccessor;
        }

        [HttpPost]
        [Route("Getting2LeggedToken")]
        public async Task<IActionResult> GetAuthToken()
        {
            var clientId = Convert.ToBase64String(Encoding.UTF8.GetBytes("8qVFzWRgPGamJsG8bGCHTop1oQRnLrbM"));
            var clientSecret = Convert.ToBase64String(Encoding.UTF8.GetBytes("sUXbNlkIipLBxjmG"));
            var client = _clientFactory.CreateClient();
            var message = new HttpRequestMessage(HttpMethod.Post, "https://developer.api.autodesk.com/authentication/v2/token");
            message.Headers.Add("accept", "application / json");

            var httpMessageBody = new
            {
                grant_type = "authorization_code",
                code = "code",
                redirect_uri = "https://localhost:8080",
                client_id = clientId,
                client_secret = clientSecret,
            };

            var serializedbody = JsonConvert.SerializeObject(httpMessageBody);
            message.Content = new StringContent(serializedbody);
            message.Content.Headers.ContentType.MediaType = "application/x-www-form-urlencoded";


            var response = await client.SendAsync(message);
            var status = response.StatusCode;
            var token = await response.Content.ReadAsStringAsync();
            return Ok(token + " & " + status);
        }






        [HttpGet]
        [Route("GettingAuthorization")]
        public async Task<IActionResult> GetAuthorization()
        {
            var clientId = "8qVFzWRgPGamJsG8bGCHTop1oQRnLrbM";
            var clientSecret = "sUXbNlkIipLBxjmG";
            var client = _clientFactory.CreateClient();

            var dictionary = new Dictionary<string, string>()
            {
                { "response_type", "code" },
                { "client_id", clientId },
                { "redirect_uri", returnUri },
                { "scope", "data:read" },
                { "state", "123" }
            };

            var uri = QueryHelpers.AddQueryString("https://developer.api.autodesk.com/authentication/v2/authorize", dictionary);
            var message = new HttpRequestMessage(HttpMethod.Get, uri);
            message.Headers.Add("accept", "application/json");
            var response = await client.SendAsync(message);

            if(response.StatusCode == HttpStatusCode.OK)
            {
                var content = await response.Content.ReadAsStringAsync();
                var contentUri = content;
                contentUri = String.Join("/", contentUri.Split("/").Select(s => System.Net.WebUtility.UrlEncode(s)));
                
                Response.Headers.Add("location" ,  contentUri);
                return View();
            }
            else
            {
                return BadRequest();
            }

        }






        [HttpGet]
        [Route("GetOIDG")]
        public async Task<IActionResult> GettingOIDG()
        {
            var client = _clientFactory.CreateClient();
            var message = new HttpRequestMessage(HttpMethod.Get,
                "https://developer.api.autodesk.com/.well-known/openid-configuration");
            var response = await client.SendAsync(message);
            var stringresponse = response.Content.ReadAsStringAsync();
            return Ok(stringresponse);
        }


    }
}
