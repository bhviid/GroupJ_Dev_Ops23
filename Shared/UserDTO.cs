namespace MiniTwit.Shared
{
    public record UserDTO(string Username, string Email, string Password);
    public record UserLoginDTO(string Username, string Password);
}

