namespace APS_WebApp.Models
{
    public class ResponseDTO
    {
        public object Result { get; set; }
        public bool issuccess { get; set; } = true;
        public string ErrorMessage { get; set; }
    }
}
