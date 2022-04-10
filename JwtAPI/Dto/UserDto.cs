namespace JwtAPI.Dto
{
    public class UserDto
    {
        public string userName { get; set; }
        public string password { get; set; }
        public string email { get; set; } = String.Empty;
        public string name { get; set; } = String.Empty;

    }

    public class LoginRequest
    {
        public string userName { get; set; }
        public string password { get; set; }
    }
}
