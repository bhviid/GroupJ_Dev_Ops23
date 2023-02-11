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
        }
        catch (System.Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        return sqliteConn;
    }
    
    private readonly ILogger<MiniTwitController> _logger;
    private readonly int _perPage = 30;

    public MiniTwitController(ILogger<MiniTwitController> logger)
    {
        _logger = logger;
        _sqliteConn = CreateConnection();
    }

    [HttpGet]
    public IActionResult GetAllMessages()
    {
        var SQL = @$"select message.*, user.* from message, user
        where message.flagged = 0 and message.author_id = user.user_id
        order by message.pub_date desc limit {_perPage}";

        return Ok(GetMsgPairData(SQL));
    }

    [HttpGet]
    [Route("/minitwit/feed/{userId}")]
    public IActionResult GetUserFeed(string userId)
    {
        //Does the userId exist?
        string SQL = @$"select message.*, user.* from message, user
        where message.flagged = 0 and message.author_id = user.user_id and (
            user.user_id = {userId} or
            user.user_id in (select whom_id from follower
                                    where who_id = {userId}))
        order by message.pub_date desc limit {_perPage}";

        return Ok(GetMsgPairData(SQL));
    }

    [HttpGet]
    [Route("/minitwit/{username}")]
    public IActionResult GetUserTimeline(string username)
    {
        string profile_userSQL = $"""select * from user where username = "{username}" """;
        Console.WriteLine(profile_userSQL);
        var sqlCmd = _sqliteConn.CreateCommand();
        sqlCmd.CommandText = profile_userSQL;
        var s = sqlCmd.ExecuteReader();
        User profileUser;
    
        if(s.Read())
        {
            profileUser = new User 
            {
                UserId = s.GetInt32(0),
                Username = s.GetString(1),
                Email = s.GetString(2),
            };
        }
        else return NotFound();

        string SQL = @$"select message.*, user.* from message, user where
            user.user_id = message.author_id and user.user_id = {profileUser.UserId}
            order by message.pub_date desc limit {_perPage}";
        
        return Ok(GetMsgPairData(SQL));
    }

    private List<MsgDataPair> GetMsgPairData(string SQLCMD)
    {
        var sqlCmd = _sqliteConn.CreateCommand();
        sqlCmd.CommandText = SQLCMD;
        var s = sqlCmd.ExecuteReader();

        var messages = new List<MsgDataPair>();
        while (s.Read())
        {
            var message = new Message()
            {
                MessageId = s.GetInt32(0),
                AuthorId = s.GetInt32(1),
                Text = s.GetString(2),
                PubDate =  new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(s.GetInt32(3)),
                Flagged = s.GetInt32(4),
            };
            var author = new Author(
                s.GetInt32(5), s.GetString(6), s.GetString(7)
            );
            messages.Add( new MsgDataPair(message,author) );
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
        return $"http://www.gravatar.com/avatar/{Convert.ToHexString(md5ed).ToLower()}?d=identicon&s={size}";
    }

    [HttpPost]
    [Route(("/register"))]
    [Consumes("application/json")]
    public string Register()
    {

        return "You were successfully registered and can login now";
    }
}
