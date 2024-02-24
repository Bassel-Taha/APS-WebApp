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
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace APS_WebApp.Controllers
{
    using System.Web;

    using APS_WebApp.Models;
    using APS_WebApp.Models.Auth;
    using Microsoft.AspNetCore.Http.HttpResults;

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
            var scope = new []{ "data:read", "data:write" , "data:create"};
            
            //here must use the FormUrlEncodedContent or the server will send back error 400 "bad request"
            var content = new FormUrlEncodedContent(new[]
            {
                    new KeyValuePair<string, string>( "grant_type" , "client_credentials"),
                    new KeyValuePair<string, string>("scope" , "data:read"),
                    new KeyValuePair<string, string>("scope" , "data:write"),
                    new KeyValuePair<string, string>("scope" , "data:create")
                });
            message.Content = content;
            message.Content.Headers.ContentType.MediaType = "application/x-www-form-urlencoded";

            // This is where the response from the Autodesk API is processed.
            var response = await client.SendAsync(message);
            var responseDTO = new ResponseDTO();
            if (response.IsSuccessStatusCode)
            {
                var temp = await response.Content.ReadAsStringAsync();
                var twoLeggedToken = new TwoLeggedToken();
                twoLeggedToken =
                    JsonConvert.DeserializeObject<TwoLeggedToken>(await response.Content.ReadAsStringAsync());
                SD.AccesToken = twoLeggedToken.access_token;
                responseDTO.Result = twoLeggedToken;
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



        // This is the GetThreeLeggedToken action, it is responsible for getting a three-legged token from the Autodesk API.
        [Route("GetThreeLeggedToken")]
        public async Task<IActionResult> GetThreeLeggedToken()
        {
            // This is where the actual request to the Autodesk API is made.
            var client = _clientFactory.CreateClient();
            var message = new HttpRequestMessage();


            //Http request message header and method
            message.Method = HttpMethod.Post;
            message.RequestUri = new Uri("https://developer.api.autodesk.com/authentication/v2/token");
            message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("aplication/json"));
            message.Headers.Add("Authorization", $"Basic {SD.Key}");


            //here must use the FormUrlEncodedContent or the server will send back error 400 "bad request"
            var content = new FormUrlEncodedContent(new[]
            {
                    new KeyValuePair<string, string>( "grant_type" , "authorization_code"),
                    new KeyValuePair<string, string>("code" , SD.AuthCode),
                    new KeyValuePair<string, string>("redirect_uri" , SD.ReturnPath)
                });

            //adding the content to the message and adding the content type to the content header 
            message.Content = content;
            message.Content.Headers.ContentType.MediaType = "application/x-www-form-urlencoded";

                
            // This is where the response from the Autodesk API is processed.
            var response = await client.SendAsync(message);
            var responseDTO = new ResponseDTO();
            if (response.IsSuccessStatusCode)
            {
                var threeLeggedToken =
                    JsonConvert.DeserializeObject<ThreeLeggedToken>(await response.Content.ReadAsStringAsync());
                SD.AuthCode = threeLeggedToken.access_token;
                SD.RefreshToken = threeLeggedToken.refresh_token;
                responseDTO.Result = threeLeggedToken;

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

            var dictionary = new Dictionary<string, List<string>>()
                                 {
                                     { "response_type", new List<string> { "code" } },
                                     { "client_id", new List<string> { clientId } },
                                     { "redirect_uri", new List<string> { "https://localhost:8080/" } },
                                     { "scope", new List<string> { "data:read", "data:write", "data:create" } },
                                     { "state", new List<string> { "123" } }
                                 };

            var queryString = string.Join("&", dictionary.SelectMany(kvp =>
                kvp.Value.Select(value => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(value)}")));

            var baseUrl = "https://developer.api.autodesk.com/authentication/v2/authorize";
            var uri = $"{baseUrl}?{queryString}";
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
