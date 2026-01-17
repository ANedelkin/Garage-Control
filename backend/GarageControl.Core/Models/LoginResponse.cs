namespace GarageControl.Core.Models
{
    public class LoginResponse
    {
        public LoginResponse(bool success, string message = "", string token = "", string refreshToken = "", List<string>? accesses = null, bool hasWorkshop = false)
        {
            Token = token;
            RefreshToken = refreshToken;
            Message = message;
            Success = success;
            Accesses = accesses;
            HasWorkshop = hasWorkshop;
        }
        public object ToResponse()
        {
            return new
            {
                Success,
                Message,
                Accesses,
                HasWorkshop
            };
        }
    
        public bool Success { get; set; }
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<string>? Accesses { get; set; }
        public bool HasWorkshop { get; set; }
    }
}