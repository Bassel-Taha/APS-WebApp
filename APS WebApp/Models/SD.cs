using System.Text;

namespace APS_WebApp.Models
{
    public class SD
    {
        public static string clientId { get; set; } = "8qVFzWRgPGamJsG8bGCHTop1oQRnLrbM";

        public static string clientSecret { get; set; } = "sUXbNlkIipLBxjmG";

        public static string Key { get; set; }= Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));

        public static string AuthCode { get; set; }

        public static string State { get; set; }

        public static string ReturnPath { get; set; } = "https://localhost:8080/";

        public static string AccesToken { get; set; }

        public string JWT { get; set; } = $"Bearer {AccesToken}"; 

    }
}
