namespace MiniTwit.Shared
{
    public record UserDTO
    {
        public string Username { get; set; }

        public string Password { get; set; }

        public string Email { get; set; }
    }

    public record UserLoginDTO(string Username, string Password);
}

