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
    using APS_WebApp.Models;

    public class AuthController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly IHttpContextAccessor _contextAccessor;

        public AuthController(IHttpClientFactory clientFactory , IHttpContextAccessor contextAccessor)
        {
            _clientFactory = clientFactory;
            _contextAccessor = contextAccessor;
        }

        [Route("GetTwoLeggedToken")]
        public async Task<IActionResult> GetTwoLeggedToken()
        {
            var key = SD.Key;
            var AuthKey = $"Basic {SD.AuthCode}";
            var client = _clientFactory.CreateClient();
            var message = new HttpRequestMessage(HttpMethod.Post, "https://developer.api.autodesk.com/authentication/v2/token");
            message.Headers.Add("accept", "application / json");

            dynamic httpMessageBody = new 
            {
                code=SD.AuthCode,
                redirect_uri=SD.ReturnPath,
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
