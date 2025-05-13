namespace chuyendoiso.DTOs
{
    public class OtpVerifyRequest
    {
        public string Username { get; set; }
        public string Otp { get; set; }
        public bool TrustDevice { get; set; }
    }
}
