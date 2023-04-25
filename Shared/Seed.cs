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

        var userFaker = new Faker<User>()
            .RuleFor(u => u.UserId, f => userId++)
            .RuleFor(u => u.Email, f => f.Person.Email)
            .RuleFor(u => u.Username, f => f.Person.UserName)
            .RuleFor(u => u.Password, f => f.Internet.Password());
        
        users.AddRange(userFaker.Generate(amountOfUsers));

        // 1 in 10 of a msg being flagged
        var flagsChance = Enumerable.Repeat(0,9).Append(1).ToArray();
        var msgFaker = new Faker<Message>()
            .RuleFor(m => m.MessageId, f => msgid++)
            .RuleFor(m => m.PubDate, f => f.Date.Recent(days: 20))
            .RuleFor(m => m.Text, f => f.Lorem.Lines(3))
            .RuleFor(m => m.AuthorId, f => f.Random.Int(0, userId))
            .RuleFor(m => m.Flagged, f => f.PickRandom(flagsChance));
        
        msgs.AddRange(msgFaker.Generate(amountOfTwits));

        ctx.Users.AddRange(users);
        ctx.Messages.AddRange(msgs);
        ctx.SaveChanges();
    }
}