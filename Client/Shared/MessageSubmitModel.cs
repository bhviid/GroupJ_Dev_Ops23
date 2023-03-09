namespace MiniTwit.Client.Shared;

public class MessageSubmitModel
{
    public string WhatsOnYourMindString { get; set; } = "";

    public void Reset() => WhatsOnYourMindString = "";
}