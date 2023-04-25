namespace MiniTwit.Shared;

public class Latest
{
    public int Id { get; set; }
    public int Value { get; set; }
    public DateTime CreatedAt { get; set; }

    public Latest(int value)
    {
        Value = value;
        CreatedAt = DateTime.Now;
    }
}