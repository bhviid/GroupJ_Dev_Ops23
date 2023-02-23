namespace MiniTwit.Shared
{
    public record UserDTO(string Username, string Email, string Password);
    public record UserLoginDTO(string Username, string Password);
    public record UserCreateDTO(string Username, string Email, string Password, string Password2);
}

