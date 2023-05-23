namespace MiniTwit.Shared;

public record MsgDataPair(Message Msg, Author Author);

public record MsgDataAndLength (int TotalLength, IEnumerable<MsgDataPair> data);