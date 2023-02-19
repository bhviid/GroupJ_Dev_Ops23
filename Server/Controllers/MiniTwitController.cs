using Microsoft.AspNetCore.Mvc;
using System.Data.SQLite;
using MiniTwit.Shared;
using Newtonsoft.Json.Linq;

namespace MiniTwit.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class MiniTwitController : ControllerBase, IDisposable
{
    SQLiteConnection _sqliteConn;
    private static DateTime Jan1970 = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

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

    public MiniTwitController(ILogger<MiniTwitController> logger, SQLiteConnection conn)
    {
        _logger = logger;
        //_sqliteConn = CreateConnection();
        _sqliteConn = conn;
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

    [HttpGet("is-follower/{whoUsername}/{whomUsername}")]
    public IActionResult IsFollower(string whoUsername, string whomUsername)
    {
        int? whoId = GetUserId(whoUsername);
        int? whomId = GetUserId(whomUsername);

        _sqliteConn.Open();
        string SQL = @$"select 1 from follower where
            follower.who_id = {whoId} and follower.whom_id = {whomId}";
        var sqlCmd = _sqliteConn.CreateCommand();
        sqlCmd.CommandText = SQL;
        var result = sqlCmd.ExecuteScalar();
        _sqliteConn.Close();
        return result is not null ? Ok(true) : Ok(false);
    }

    [HttpGet]
    [Route("/minitwit/{username}")]
    public IActionResult GetUserTimeline(string username)
    {
        _sqliteConn.Open();
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
        _sqliteConn.Close();

        string SQL = @$"select message.*, user.* from message, user where
            user.user_id = message.author_id and user.user_id = {profileUser.UserId}
            order by message.pub_date desc limit {_perPage}";
        
        return Ok(GetMsgPairData(SQL));
    }

    [HttpPost]
    [Route("{username}/follow")]
    [Consumes("application/json")]
    public IActionResult Follow(string username, User activeUser)
    {
        Console.WriteLine("yo");
        var whomId = GetUserId(username);
        if(whomId is null) return NotFound();

        _sqliteConn.Open();
        var SQL = @$"insert into follower (who_id, whom_id) values ({activeUser.UserId}, {whomId})";
        Console.WriteLine(SQL);
        var sqlCmd = _sqliteConn.CreateCommand();
        sqlCmd.CommandText = SQL;
        sqlCmd.ExecuteNonQuery();
        _sqliteConn.Close();
        return Ok($"You are now following {username}");
    }

    [HttpPost]
    [Route("{username}/unfollow")]
    [Consumes("application/json")]
    public IActionResult UnFollow(string username, User activeUser)
    {
        var whomId = GetUserId(username);
        if(whomId is null) return NotFound();

        _sqliteConn.Open();
        var SQL = @$"delete from follower where who_id={activeUser.UserId} and whom_id={whomId}";
        Console.WriteLine(SQL);
        var sqlCmd = _sqliteConn.CreateCommand();
        sqlCmd.CommandText = SQL;
        sqlCmd.ExecuteNonQuery();
        _sqliteConn.Close();
        return Ok($"You are no longer following {username}");
    }

    [HttpPost]
    [Route("add-message")]
    [Consumes("application/json")]
    public IActionResult AddMessage(MessageCreateDTO message)
    {
        var authorId = GetUserId(message.Author);
        if(authorId is null) return BadRequest();

        _sqliteConn.Open();

        var currTimeInSecSince1970 = (int) (DateTime.Now - Jan1970).TotalSeconds;

        var SQL = $"insert into message (author_id, text, pub_date, flagged) values (@aid, @text, {currTimeInSecSince1970}, 0)";
        var sqlCmd = _sqliteConn.CreateCommand();
        sqlCmd.CommandText = SQL;
        sqlCmd.Parameters.AddWithValue("@aid", authorId);
        sqlCmd.Parameters.AddWithValue("@text", message.Text);

        sqlCmd.ExecuteNonQuery();

        _sqliteConn.Close();
        return Ok($"Message posted: {message.Text}");
    }
    private int? GetUserId(string username)
    {
        _sqliteConn.Open();
        string SQL = $"""select user_id from user where username = "{username}" """;
        var sqlCmd = _sqliteConn.CreateCommand();
        sqlCmd.CommandText = SQL;
        var s = sqlCmd.ExecuteScalar();
        _sqliteConn.Close();
        return s is not null ? Int32.Parse(s.ToString()) : null;
    }

    private List<MsgDataPair> GetMsgPairData(string SQLCMD)
    {
        _sqliteConn.Open();
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
                s.GetInt32(5), s.GetString(6), s.GetString(7), Md5Hash(s.GetString(7))
            );
            messages.Add( new MsgDataPair(message,author) );
        }
        _sqliteConn.Close();
        return messages;
    }

    [HttpGet]
    [Route("md5/{email}/{size}")]
    public string Md5Hash(string email, int size = 48)
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
        await _sqliteConn.OpenAsync();
        if (await UserExists(user))
        {
            await _sqliteConn.CloseAsync();
            return Conflict("User already exists");
        }

        using var md5 = System.Security.Cryptography.MD5.Create();
        var md5ed = md5.ComputeHash(System.Text.Encoding.ASCII.GetBytes(user.Password));
        var PwHash = System.Text.Encoding.UTF8.GetString(md5ed);

        var sqlcmd = _sqliteConn.CreateCommand();
        sqlcmd.CommandText = $@"INSERT INTO user
        (username, email, pw_hash) VALUES
        ('{user.Username}', '{user.Email}', '{PwHash}');";
        var res = await sqlcmd.ExecuteNonQueryAsync();
        
        var createdUserCmd = _sqliteConn.CreateCommand();
        createdUserCmd.CommandText = @$"SELECT * FROM user WHERE
        email LIKE '{user.Email}' AND username LIKE '{user.Username}'";
        
        var userReader = await createdUserCmd.ExecuteReaderAsync();
        await userReader.ReadAsync();
        User createdUser = new User{UserId = (int)(long)userReader["user_id"], 
        Email = (string)userReader["email"],
        Username = (string)userReader["username"],
        Password = (string)userReader["pw_hash"]};
        await _sqliteConn.CloseAsync();
        if (res == 1)
        {
            return Created($"user/{createdUser.UserId}", createdUser);
        }
        return BadRequest("Nothing was changed");
    }

    private async Task<bool> UserExists(UserDTO user)
    {
        var existsCommand = _sqliteConn.CreateCommand();
        existsCommand.CommandText = @$"SELECT COUNT(user_id)
        FROM user WHERE
        username LIKE '{user.Username}'";
        var res = Convert.ToInt64(await existsCommand.ExecuteScalarAsync());
        return res > 0;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(UserLoginDTO loginData)
    {
        _sqliteConn.Open();

        var sql = "select * from user where username = @uname";
        var cmd = _sqliteConn.CreateCommand();
        cmd.CommandText = sql;
        cmd.Parameters.AddWithValue("@uname",loginData.Username);
        var reader = await cmd.ExecuteReaderAsync();
        
        if(!reader.Read())
        {
            _sqliteConn.Close();
            return StatusCode(StatusCodes.Status401Unauthorized, "Invalid username");
        }
        UserDTO userInDb = new(){
            Email = (string)reader["email"],
            Username = (string)reader["username"],
            Password = (string)reader["pw_hash"]
        };
        _sqliteConn.Close();

        // hash the password from Post/request
        using var md5 = System.Security.Cryptography.MD5.Create();
        var md5ed = md5.ComputeHash(System.Text.Encoding.ASCII.GetBytes(loginData.Password));
        var PwHash = System.Text.Encoding.UTF8.GetString(md5ed);
        //check if the hash from db matches the hash from post/request.
        if(userInDb.Password != PwHash){
            return StatusCode(StatusCodes.Status401Unauthorized, "Invalid password");
        }
        return Ok(userInDb);
    }

    public void Dispose()
    {
        //_sqliteConn.Close();
        Console.WriteLine("connection was closed");
    }
}
