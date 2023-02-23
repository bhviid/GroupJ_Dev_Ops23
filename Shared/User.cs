namespace MiniTwit.Shared;
using System.ComponentModel.DataAnnotations.Schema;
public class User
{
    [Column("user_id")]
    public int UserId { get; set; }

    [Column("username")]
    public string Username { get; set; }

    [Column("pw_hash")]
    public string Password { get; set; }

    [Column("email")]
    public string Email { get; set; }
}