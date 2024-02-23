using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using System.Text;
using System;
using System.IO;
using System.Net;
using System.Net.Http.Headers;
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

        public AuthController(IHttpClientFactory clientFactory, IHttpContextAccessor contextAccessor)
        {
            _clientFactory = clientFactory;
            _contextAccessor = contextAccessor;
        }

        // This is the GetTwoLeggedToken action, it is responsible for getting a two-legged token from the Autodesk API.
        [Route("GetTwoLeggedToken")]
        public async Task<IActionResult> GetTwoLeggedToken()
        {
            // This is where the actual request to the Autodesk API is made.
            var client = _clientFactory.CreateClient();
            var message = new HttpRequestMessage();
            message.Method = HttpMethod.Post;
            message.RequestUri = new Uri("https://developer.api.autodesk.com/authentication/v2/token");
            message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("aplication/json"));
            message.Headers.Add("Authorization", $"Basic {SD.Key}");
            var grant_type = "client_credentials";
            var scope = "data:read";
            var contentsKeyValuePairs = new[]
            {
                    new KeyValuePair<string , string>("grant_type" , "client_credentials"),
                    new KeyValuePair<string , string>("scope" , "data:read")
                };
            //here must use the FormUrlEncodedContent or the server will send back error 400 "bad request"
            var content = new FormUrlEncodedContent(new[]
            {
                    new KeyValuePair<string, string>( "grant_type" , "client_credentials"),
                    new KeyValuePair<string, string>("scope" , "data:read")
                });
            message.Content = content;
            message.Content.Headers.ContentType.MediaType = "application/x-www-form-urlencoded";

            // This is where the response from the Autodesk API is processed.
            var response = await client.SendAsync(message);
            var responseDTO = new ResponseDTO();
            if (response.IsSuccessStatusCode)
            {
                responseDTO.Result = await response.Content.ReadAsStringAsync();
            }
            else
            {
                responseDTO.ErrorMessage = response.StatusCode.ToString();
                responseDTO.Result = await response.Content.ReadAsStringAsync();
                responseDTO.issuccess = false;
            }
            client.Dispose();

            // The result is then returned to the caller.
            return View(responseDTO);
        }

        // This is the GetAuthorization action, it is responsible for getting an authorization code from the Autodesk API.
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

            //giving the dictionary to the queryHelper to convert it to query and adding it to the base url from Autodesk to send the parameters in the URL
            var uri = QueryHelpers.AddQueryString("https://developer.api.autodesk.com/authentication/v2/authorize", dictionary);
            Response.Headers.Add("location", uri);

            return new StatusCodeResult(303);
        }

        // This is the GettingOIDG action, it is responsible for getting the OpenID configuration from the Autodesk API.
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
