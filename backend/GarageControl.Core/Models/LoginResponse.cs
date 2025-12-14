namespace GarageControl.Core.Models
{
    public class LoginResponse
    {

        public LoginResponse()
        {
            
        }

        public LoginResponse(string message, bool success)
        {
            Message = message;
            Success = success;
            Accesses = new List<string>();
        }

        public LoginResponse(string username, string token, string refreshToken, string? message)
        {
            Username = username;
            Token = token;
            RefreshToken = refreshToken;
            Message = message ?? string.Empty;
            Accesses = new List<string>();
        }

        public LoginResponse(string username, string token, string refreshToken, string? message, bool success)
        {
            Username = username;
            Token = token;
            RefreshToken = refreshToken;
            Message = message ?? string.Empty;
            Success = success;
            Accesses = new List<string>();
        }

        public LoginResponse(string username, string token, string refreshToken, string? message, bool success, List<string> accesses)
        {
            Username = username;
            Token = token;
            RefreshToken = refreshToken;
            Message = message ?? string.Empty;
            Success = success;
            Accesses = accesses;
        }

        public bool Success { get; set; }
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public List<string> Accesses { get; set; } = new List<string>();
    }
}