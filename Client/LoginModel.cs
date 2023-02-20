namespace MiniTwit.Client;

public class LoginModel
{
    public string Username { get; set; } = "";

    public string Password { get; set; } = "";

    public void Reset()
    {
        Username = "";
        Password = "";
    }
}