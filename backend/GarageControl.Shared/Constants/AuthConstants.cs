namespace GarageControl.Shared.Constants
{
    public static class AuthConstants
    {
        public const int emailMinLength = 5;
        public const int emailMaxLength = 50;
        public const int passwordMinLength = 6;
        public const int passwordMaxLength = 50;
        public const string emailRegex = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
    }
}