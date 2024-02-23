﻿using Microsoft.AspNetCore.Mvc;
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

        [Route("GetTwoLeggedToken")]
        public async Task<IActionResult> GetTwoLeggedToken()
        {
            #region MyCode


            var client = _clientFactory.CreateClient();
            var message = new HttpRequestMessage();
            message.Method = HttpMethod.Post;
            message.RequestUri = new Uri("https://developer.api.autodesk.com/authentication/v2/token");
            message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("aplication/json"));
            message.Headers.Add("Authorization", $"Basic {SD.Key}");
            //var grant_type = "client_credentials";
            //var scope = "data:read";
            //var content = new
            //{
            //    grant_type,
            //    scope
            //};
            var test = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>( "grant_type" , "client_credentials"),
                new KeyValuePair<string, string>("scope" , "data:read")
            });
            string serializedcontent = JsonConvert.SerializeObject(test);
           // message.Content = new StringContent(JsonConvert.SerializeObject(test));
            message.Content =test;
            message.Content.Headers.ContentType.MediaType = "application/x-www-form-urlencoded";

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



            #endregion

            return View(responseDTO);
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
