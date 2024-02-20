using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using System.Text;
using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Security.Principal;
using static System.Net.WebRequestMethods;

namespace APS_WebApp.Controllers
{
    public class AuthController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly string returnUri = "https://localhost:8080/";
        public string AuthCode { get; set; }

        public AuthController(IHttpClientFactory clientFactory , IHttpContextAccessor contextAccessor)
        {
            _clientFactory = clientFactory;
            _contextAccessor = contextAccessor;
        }

        [Route("GetTwoLeggedToken")]
        public async Task<IActionResult> GetTwoLeggedToken()
        {
            var clientId = "8qVFzWRgPGamJsG8bGCHTop1oQRnLrbM";
            var clientSecret ="sUXbNlkIipLBxjmG";
            var key = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));
            var AuthKey = $"Basic {key}";
            var client = _clientFactory.CreateClient();
            var message = new HttpRequestMessage(HttpMethod.Post, "https://developer.api.autodesk.com/authentication/v2/token");
            message.Headers.Add("accept", "application / json");

            dynamic httpMessageBody = new 
            {
                code="zwey86d1yeHb6rfa8BcugJy2hEY6a-o8WLRU5JSq",
                redirect_uri="https://localhost:8080/",
                grant_type = "client_credentials"
            };

            var serializedbody = JsonConvert.SerializeObject(httpMessageBody);
            message.Content = new StringContent(serializedbody);
            message.Content.Headers.ContentType.MediaType = "application/x-www-form-urlencoded";
            message.Headers.Add("Authorization", AuthKey);
            message.Headers.Add("grant_type" , "client_credentials");

            var response = await client.SendAsync(message);
            var status = response.StatusCode;
            var token = await response.Content.ReadAsStringAsync();
            client.Dispose();
            return Ok(token + " & " + status);
        }

        //[Route("https://localhost:8080/?code={code}&state={state}")]
        public async Task<IActionResult> GetAuthCode()
        {
            var context = Request.RouteValues.Values;
            return Redirect("https://localhost:8080/");
        }




        [HttpGet]
        [Route("GettingAuthorization")]
        public async Task<IActionResult> GetAuthorization()
        {
            var clientId = "8qVFzWRgPGamJsG8bGCHTop1oQRnLrbM";

            var dictionary = new Dictionary<string, string>()
            {
                { "response_type", "code" },
                { "client_id", clientId },
                { "redirect_uri", "https://localhost:8080/" },
                { "scope", "data:read" },
                { "state", "123" }
            };

            var uri = QueryHelpers.AddQueryString("https://developer.api.autodesk.com/authentication/v2/authorize", dictionary);
            Response.Headers.Add("location", uri);
            return new StatusCodeResult(303);
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
            client.Dispose();
            return Ok(stringresponse);
        }


    }
}
