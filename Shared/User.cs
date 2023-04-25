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

    public ICollection<Message> Messages { get; set; } = new List<Message>();

    public ICollection<Follows> Follows { get; set; } = new List<Follows>();
    public ICollection<Follows> FollowedBy { get; set; } = new List<Follows>();
}