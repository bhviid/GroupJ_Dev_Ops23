using Microsoft.AspNetCore.Mvc;
using System.Data.SQLite;
using MiniTwit.Shared;

namespace MiniTwit.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class MiniTwitController : ControllerBase
{
    SQLiteConnection _sqliteConn;

    static SQLiteConnection CreateConnection()
    {
        var sqliteConn = new SQLiteConnection("Data Source=../tmp/minitwit.db;Version=3;");
        try
        {
            sqliteConn.Open();
            Console.WriteLine("Database connection established");
        }
        catch (System.Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        return sqliteConn;
    }
    
    private readonly ILogger<MiniTwitController> _logger;

    public MiniTwitController(ILogger<MiniTwitController> logger)
    {
        _logger = logger;
        _sqliteConn = CreateConnection();
    }

    [HttpGet]
    public IEnumerable<Message> GetAllMessages()
    {
        var perPage = 30;
        var SQL = @$"select message.*, user.* from message, user
        where message.flagged = 0 and message.author_id = user.user_id
        order by message.pub_date desc limit {perPage}";

        var sqlCmd = _sqliteConn.CreateCommand();
        sqlCmd.CommandText = SQL;
        var s = sqlCmd.ExecuteReader();
        
        var messages = new List<Message>();
        while (s.Read())
        {
            var message = new Message()
            {
                MessageId = s.GetInt32(0),
                AuthorId = s.GetInt32(1),
                Text = s.GetString(2),
                PubDate =  new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(s.GetInt32(3)),
                Flagged = s.GetInt32(4)
            };
            messages.Add(message);
        }
        return messages;
    }

    [HttpGet]
    [Route("md5/{email}/{size}")]
    public string Md5Hash(string email, int size)
    {
        //Must be here since MD5 is disabled in blazor wasm...
        using var md5 = System.Security.Cryptography.MD5.Create();
        byte[] md5ed = md5.ComputeHash(System.Text.Encoding.ASCII.GetBytes(email.Trim().ToLower()));
        return $"http://www.gravatar.com/avatar/{Convert.ToHexString(md5ed)}?d=identicon&s={size}";
    }

    [HttpPost]
    [Route(("/register"))]
    [Consumes("application/json")]
    public string Register()
    {

        return "You were successfully registered and can login now";
    }
}
