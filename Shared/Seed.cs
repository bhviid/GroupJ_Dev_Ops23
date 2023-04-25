using Bogus;

namespace MiniTwit.Shared;

public static class Seed
{
    public static void InitFakeData(this TwitContext ctx, int amountOfUsers = 1000, int amountOfTwits = 10000)
    {
        var msgs = new List<Message>();
        var users = new List<User>();
        var msgid = 1;
        var userId = 1;

        // 1 in 10 of a msg being flagged
        var flagsChance = Enumerable.Repeat(0,9).Append(1).ToArray();
        var msgFaker = new Faker<Message>()
            .RuleFor(m => m.MessageId, f => msgid++)
            .RuleFor(m => m.PubDate, f => f.Date.Recent(days: 20))
            .RuleFor(m => m.Text, f => f.Lorem.Lines(3))
            .RuleFor(m => m.Flagged, f => f.PickRandom(flagsChance));

        var userFaker = new Faker<User>()
            .RuleFor(u => u.UserId, f => userId++)
            .RuleFor(u => u.Email, f => f.Person.Email)
            .RuleFor(u => u.Username, f => f.Person.UserName)
            .RuleFor(u => u.Password, f => f.Internet.Password())
            .RuleFor(u => u.Messages, (f, u) => {
                msgFaker.RuleFor(m => m.AuthorId, _ => u.UserId)
                .RuleFor(m => m.User, _ => u);
                    var messages = msgFaker.GenerateBetween(5, 10);
                    //ctx.Messages.AddRange(messages);
                    msgs.AddRange(messages);
                    return null!;
            });
        
        users.AddRange(userFaker.Generate(amountOfUsers));

        ctx.Users.AddRange(users);
        ctx.Messages.AddRange(msgs);
        ctx.SaveChanges();
    }
}