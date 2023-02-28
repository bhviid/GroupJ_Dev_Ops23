namespace MiniTwit.Shared
{
    public record MessageDTO(int AuthorId, string Text);
    public record MessageCreateDTO(string Author, string Text);

}
