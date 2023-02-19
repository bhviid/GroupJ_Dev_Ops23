using MiniTwit.Shared;
namespace MiniTwit.Client;

public class UserService
{
    public User? ActiveUser { get; set; }

    public bool IsSignedIn() => ActiveUser is not null;
}