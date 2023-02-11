namespace MiniTwit.Shared;

public class User
{
    public int UserId { get; set; }
    public string Username { get; set; }

    public string PwHash { get; set; }

    public string Email { get; set; }
}