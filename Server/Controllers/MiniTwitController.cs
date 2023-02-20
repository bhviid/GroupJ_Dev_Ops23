using Microsoft.AspNetCore.Mvc;
using System.Data.SQLite;
using MiniTwit.Shared;

namespace MiniTwit.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class MiniTwitController : ControllerBase, IDisposable
{
    TwitContext _db;
    // SQLiteConnection _sqliteConn;
    private static DateTime Jan1970 = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

    // static SQLiteConnection CreateConnection()
    // {
    //     var sqliteConn = new SQLiteConnection("Data Source=../tmp/minitwit.db;Version=3;");
    //     try
    //     {
    //         sqliteConn.Open();
    //     }
    //     catch (System.Exception ex)
    //     {
    //         Console.WriteLine(ex.Message);
    //     }
    //     return sqliteConn;
    // }
    
    private readonly ILogger<MiniTwitController> _logger;
    private readonly int _perPage = 30;

    public MiniTwitController(ILogger<MiniTwitController> logger, TwitContext db)
    {
        _logger = logger;
        //_sqliteConn = CreateConnection();
        _db = db;
    }

    [HttpGet]
    public IActionResult GetAllMessages()
    {
        // var SQL = @$"select message.*, user.* from message, user
        // where message.flagged = 0 and message.author_id = user.user_id
        // order by message.pub_date desc limit {_perPage}";

        var result = (from m in _db.Messages
        join u in _db.Users on m.AuthorId equals u.UserId
        where m.Flagged == 0
        orderby m.PubDate descending
        select new MsgDataPair(m, new Author(u.UserId, u.Username, u.Email))).Take(_perPage);
        return Ok(result);
    }

    [HttpGet]
    [Route("/minitwit/feed/{userId}")]
    public IActionResult GetUserFeed(int userId)
    {
        //Does the userId exist?
        // string SQL = @$"select message.*, user.* from message, user
        // where message.flagged = 0 and message.author_id = user.user_id and (
        //     user.user_id = {userId} or
        //     user.user_id in (select whom_id from follower
        //                             where who_id = {userId}))
        // order by message.pub_date desc limit {_perPage}";
        
        // Pretty sure this forces the query to be executed in memory rather than on db
        //makes it a ton faster, from approx 2sec to <10ms
        var flws = _db.Followings.Where(f => f.who_id == userId).Select(f => f.whom_id).ToList();

        var result = (from m in _db.Messages
        join u in _db.Users on m.AuthorId equals u.UserId
        where m.Flagged == 0 && (
            u.UserId == userId || flws.Contains(u.UserId)
        )
        orderby m.PubDate descending
        select new MsgDataPair(m, new Author(u.UserId, u.Username, u.Email))).Take(_perPage);

        return Ok(result);
    }

    [HttpGet("is-follower/{whoUsername}/{whomUsername}")]
    public async Task<IActionResult> IsFollower(string whoUsername, string whomUsername)
    {
        int? whoId = GetUserId(whoUsername);
        int? whomId = GetUserId(whomUsername);

        var result = from f in _db.Followings where f.who_id == whoId && f.whom_id == whomId select f;
        
        return result is not null ? Ok(true) : Ok(false);
    }

    [HttpGet]
    [Route("/minitwit/{username}")]
    public IActionResult GetUserTimeline(string username)
    {
        
        var user = (User)(from u in _db.Users where u.Username == username select u);
        // string profile_userSQL = $"""select * from user where username = "{username}" """;
        // Console.WriteLine(profile_userSQL);
        // var sqlCmd = _sqliteConn.CreateCommand();
        // sqlCmd.CommandText = profile_userSQL;
        // var s = sqlCmd.ExecuteReader();
        // User profileUser;
    
        // if(s.Read())
        // {
        //     profileUser = new User 
        //     {
        //         UserId = s.GetInt32(0),
        //         Username = s.GetString(1),
        //         Email = s.GetString(2),
        //     };
        // }
        if (user is null)
        {
            return NotFound();
        }
        var timeline = (from m in _db.Messages 
        join u in _db.Users on m.AuthorId equals u.UserId 
        orderby m.PubDate descending 
        select new MsgDataPair (m, new Author(u.UserId, u.Username, u.Email))).Take(_perPage);

        // string SQL = @$"select message.*, user.* from message, user where
        //     user.user_id = message.author_id and user.user_id = {profileUser.UserId}
        //     order by message.pub_date desc limit {_perPage}";
        
        return Ok(timeline);
    }

    [HttpPost]
    [Route("{username}/follow")]
    [Consumes("application/json")]
    public async Task<IActionResult> Follow(string username, User activeUser)
    {
        Console.WriteLine("yo");
        var whomId = GetUserId(username);
        if(whomId is null) return NotFound();

        // _sqliteConn.Open();
        // var SQL = @$"insert into follower (who_id, whom_id) values ({activeUser.UserId}, {whomId})";
        // Console.WriteLine(SQL);
        await _db.Followings.AddAsync(new Follows{who_id = activeUser.UserId, whom_id = (int)whomId});
        await _db.SaveChangesAsync();
        // var sqlCmd = _sqliteConn.CreateCommand();
        // sqlCmd.CommandText = SQL;
        // sqlCmd.ExecuteNonQuery();
        // _sqliteConn.Close();
        return Ok($"You are now following {username}");
    }

    [HttpPost]
    [Route("{username}/unfollow")]
    [Consumes("application/json")]
    public IActionResult UnFollow(string username, User activeUser)
    {
        Console.WriteLine("yo");
        var whomId = GetUserId(username);
        if(whomId is null) return NotFound();
        var toRemove = (from f in _db.Followings
        where f.who_id == activeUser.UserId && f.whom_id == whomId
        select f).FirstOrDefault();
        if (toRemove is null)
        {
            return NotFound("You are not following this user");
        }
        _db.Followings.Remove(toRemove);
        _db.SaveChanges();
        // _sqliteConn.Open();
        // var SQL = @$"delete from follower where who_id={activeUser.UserId} and whom_id={whomId}";
        // Console.WriteLine(SQL);
        // var sqlCmd = _sqliteConn.CreateCommand();
        // sqlCmd.CommandText = SQL;
        // sqlCmd.ExecuteNonQuery();
        // _sqliteConn.Close();
        return Ok($"You are no longer following {username}");
    }

    [HttpPost]
    [Route("add-message")]
    [Consumes("application/json")]
    public async Task<IActionResult> AddMessage(MessageDTO message)
    {
        // _sqliteConn.Open();

        // var currTimeInSecSince1970 = DateTime.Now - Jan1970;

        // var SQL = $"""insert into message (author_id, text, pub_date, flagged) values ({message.AuthorId}, "{message.Text}", {(int)currTimeInSecSince1970.TotalSeconds}, 0)""";
        // var sqlCmd = _sqliteConn.CreateCommand();
        // sqlCmd.CommandText = SQL;
        // sqlCmd.ExecuteNonQuery();

        // _sqliteConn.Close();

        await _db.Messages.AddAsync(new Message{MessageId = 0,
        AuthorId = message.AuthorId,
        Text = message.Text,
        PubDate = DateTime.Now,
        Flagged = 0});
        await _db.SaveChangesAsync();
        return Ok($"Message posted: {message.Text}");
    }
    private int? GetUserId(string username)
    {
        // _sqliteConn.Open();
        // string SQL = $"""select user_id from user where username = "{username}" """;
        // var sqlCmd = _sqliteConn.CreateCommand();
        // sqlCmd.CommandText = SQL;
        // var s = sqlCmd.ExecuteScalar();
        // _sqliteConn.Close();
        var s = from u in _db.Users 
        where u.Username == username
        select u.UserId;
        return s is not null ? Int32.Parse(s.ToString()!) : null;
    }

    // private List<MsgDataPair> GetMsgPairData(string SQLCMD)
    // {
    //     _sqliteConn.Open();
    //     var sqlCmd = _sqliteConn.CreateCommand();
    //     sqlCmd.CommandText = SQLCMD;
    //     var s = sqlCmd.ExecuteReader();

    //     var messages = new List<MsgDataPair>();
    //     while (s.Read())
    //     {
    //         var message = new Message()
    //         {
    //             MessageId = s.GetInt32(0),
    //             AuthorId = s.GetInt32(1),
    //             Text = s.GetString(2),
    //             PubDate =  new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(s.GetInt32(3)),
    //             Flagged = s.GetInt32(4),
    //         };
    //         var author = new Author(
    //             s.GetInt32(5), s.GetString(6), s.GetString(7)
    //         );
    //         messages.Add( new MsgDataPair(message,author) );
    //     }
    //     _sqliteConn.Close();
    //     return messages;
    // }

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
    [Route(("register"))]
    [Consumes("application/json")]
    public async Task<IActionResult> Register(UserDTO user)
    {
        // await _sqliteConn.OpenAsync();
        // if (await UserExists(user))
        // {
        //     await _sqliteConn.CloseAsync();
        //     return Conflict("User already exists");
        // }

        using var md5 = System.Security.Cryptography.MD5.Create();
        var md5ed = md5.ComputeHash(System.Text.Encoding.ASCII.GetBytes(user.Password));
        var PwHash = System.Text.Encoding.UTF8.GetString(md5ed);

        // var sqlcmd = _sqliteConn.CreateCommand();
        // sqlcmd.CommandText = $@"INSERT INTO user
        // (username, email, pw_hash) VALUES
        // ('{user.Username}', '{user.Email}', '{PwHash}');";
        
        if (UserExists(user)) return Conflict("User already exists");

        await _db.Users.AddAsync(new User{UserId = 0,
        Username = user.Username,
        Password = PwHash,
        Email = user.Email});
        await _db.SaveChangesAsync();
        var createdUser = GetUser(user.Username);
        // createdUserCmd.CommandText = @$"SELECT * FROM user WHERE
        // email LIKE '{user.Email}' AND username LIKE '{user.Username}'";
        
        // var userReader = await createdUserCmd.ExecuteReaderAsync();
        // await userReader.ReadAsync();
        // User createdUser = new User{UserId = (int)(long)userReader["user_id"], 
        // Email = (string)userReader["email"],
        // Username = (string)userReader["username"],
        // Password = (string)userReader["pw_hash"]};
        // await _sqliteConn.CloseAsync();
        if (createdUser is not null)
        {
            return Created($"user/{createdUser.UserId}", createdUser);
        }
        return BadRequest("Nothing was changed");
    }

    
    public User GetUser(string username)
    {
        Console.WriteLine(username);

        var user = (from u in _db.Users
        where u.Username == username
        select u).FirstOrDefault();
        return user!;
    }

    private bool UserExists(UserDTO user)
    {
        // var existsCommand = _sqliteConn.CreateCommand();
        // existsCommand.CommandText = @$"SELECT COUNT(user_id)
        // FROM user WHERE
        // username LIKE '{user.Username}'";
        // var res = Convert.ToInt64(await existsCommand.ExecuteScalarAsync());
        
        return GetUser(user.Username) is not null;
    }

    public void Dispose()
    {
        //_sqliteConn.Close();
        Console.WriteLine("connection was closed");
    }
}
