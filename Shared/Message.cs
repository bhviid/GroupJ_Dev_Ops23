namespace MiniTwit.Shared;
using System.ComponentModel.DataAnnotations.Schema;
public class Message
{
    [Column("message_id")]
    public int MessageId { get; set; }
    [Column("author_id")]
    public int AuthorId { get; set; }
    [Column("text")]
    public string Text { get; set; }
    [Column("pub_date")]
    public DateTime? PubDate { get; set; }
    [Column("flagged")]
    public int? Flagged { get; set; }
    
    public User User { get; set; }
}