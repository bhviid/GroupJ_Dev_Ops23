namespace MiniTwit.Shared;
public class Message
{
    public int MessageId { get; set; }

    public int AuthorId { get; set; }

    public string Text { get; set; }

    public DateTime? PubDate { get; set; }

    public int? Flagged { get; set; }
}