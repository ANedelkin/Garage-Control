namespace GarageControl.Shared.Constants
{
    public static class GenericConstants
    {
        public const int phoneMaxLength = 25;
        public const int emailMaxLength = 254;
        public const int addressMinLength = 5;
        public const int addressMaxLength = 200;
        public const string colorRegex = @"^#([0-9a-fA-F]{6})$";
        public const string phoneRegex = @"^\+?[1-9]\d{1,14}$";
        public const string emailRegex = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
    }
}