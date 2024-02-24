namespace APS_WebApp.Models.Auth
{
    public class TwoLeggedToken
    {

        public string access_token { get; set; }
        public string token_type { get; set; }
        public int expires_in { get; set; }

    }
}
