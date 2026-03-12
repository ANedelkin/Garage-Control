using System.Text.Json.Serialization;

namespace GarageControl.Core.ViewModels.Shared
{
    public class MethodResponseVM
    {
        public MethodResponseVM(bool success, string? message = null, object? data = null, string? returnUrl = null, string? token = null, string? refreshToken = null, Dictionary<string, List<string>>? errors = null)
        {
            Success = success;
            Message = message;
            Data = data;
            ReturnUrl = returnUrl;
            Token = token;
            RefreshToken = refreshToken;
            Errors = errors;
        }

        [JsonIgnore]
        public bool Success { get; set; }
        public string? Message { get; set; }
        public object? Data { get; set; }
        public string? ReturnUrl { get; set; }
        public string? Token { get; set; } = string.Empty;
        public string? RefreshToken { get; set; } = string.Empty;
        public Dictionary<string, List<string>>? Errors { get; set; }
    }
}
