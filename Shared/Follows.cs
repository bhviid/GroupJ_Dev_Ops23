namespace MiniTwit.Shared;
public class Follows
{
    public int who_id { get; set; }
    public int whom_id { get; set; }
    public User Follower { get; set; }
    public User Followed { get; set; }
}