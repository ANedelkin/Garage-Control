namespace GarageControl.Core.ViewModels.Auth
{
    public class LoginResponseVM
    {
        public LoginResponseVM(bool success, string message = "", string token = "", string refreshToken = "", List<string>? accesses = null, bool hasWorkshop = false, string? userId = null, string? workerId = null)
        {
            Token = token;
            RefreshToken = refreshToken;
            Message = message;
            Success = success;
            Accesses = accesses;
            HasWorkshop = hasWorkshop;
            UserId = userId;
            WorkerId = workerId;
        }
        public object ToResponse()
        {
            return new
            {
                Success,
                Message,
                Accesses,
                HasWorkshop,
                UserId,
                WorkerId
            };
        }
    
        public bool Success { get; set; }
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<string>? Accesses { get; set; }
        public bool HasWorkshop { get; set; }
        public string? UserId { get; set; }
        public string? WorkerId { get; set; }
    }
}
