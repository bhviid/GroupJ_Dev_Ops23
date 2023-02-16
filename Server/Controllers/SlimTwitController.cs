using Microsoft.AspNetCore.Mvc;
using System.Data.SQLite;

namespace MiniTwit.Server;

[ApiController]
[Route("[controller]/[action]")]
public class SlimTwitController : ControllerBase, IDisposable
{
    SQLiteConnection _conn;

    public static int _latest;
    
    private int TimeSince1970 => (int)(DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;

    private IActionResult RequestNotFromSimulatorResponse => 
            Problem(
                detail: "You are not authorized to use this resource!",
                statusCode: StatusCodes.Status403Forbidden
            );

    public SlimTwitController(SQLiteConnection conn)
    {
        _conn = conn;
        _conn.Open();
    } 

    public void Dispose()
    {
        _conn.Close();
    }
    private bool IsRequestFromSimulator(HttpRequest req)
    {
        var reqAuth = req.Headers["Authorization"];
        return reqAuth.ToString() == "Basic c2ltdWxhdG9yOnN1cGVyX3NhZmUh";
    }

    private void updateLatest(HttpRequest req)
    {
        // this gonna need some more thinking
        int newLatest = 
            Int32.TryParse(req.Query["latest"], out newLatest)
            ? newLatest
            : -1;
        _latest = newLatest;
        //Console.WriteLine(_latest);
    }

    //Code duplication
    private int? GetUserId(string username)
    {
        string SQL = $"""select user_id from user where username = "{username}" """;
        var sqlCmd = _conn.CreateCommand();
        sqlCmd.CommandText = SQL;
        var s = sqlCmd.ExecuteScalar();
        return s is not null ? Int32.Parse(s.ToString()):null;
    }

    [HttpGet]
    public IActionResult Latest()
    {
        return Ok(new LatestInfo(_latest));
    }

    [HttpPost]
    public IActionResult Register(RegisterInfo userInfo)
    {
        updateLatest(Request);

        string? err = null;

        if(userInfo.username is null) 
            err = ("You have to enter a username");
        else if(userInfo.email is null) 
            err = ("You have to enter a valid email address");
        else if(userInfo.pwd is null)
            err = ("You have to enter a password");
        else if(GetUserId(userInfo.username) is not null)
            err = ("The username is already taken");

        if(err is not null)
        {
            return BadRequest(err);
        }

        string sql = $"""INSERT INTO user (username, email, pw_hash) VALUES (@uname,@email, @pwd) """;

        var sqlCmd = _conn.CreateCommand();
        sqlCmd.CommandText = sql;
        sqlCmd.Parameters.AddWithValue("@uname",userInfo.username);
        sqlCmd.Parameters.AddWithValue("@email",userInfo.email);
        sqlCmd.Parameters.AddWithValue("@pwd",userInfo.pwd);
        sqlCmd.ExecuteNonQuery();

        Console.WriteLine("SQL ran");

        return NoContent();
    }

    [HttpGet]
    public IActionResult Msgs()
    {
        Console.WriteLine("someone hit msgs");
        updateLatest(Request);

        if(!IsRequestFromSimulator(Request))
        {
            return RequestNotFromSimulatorResponse;
        }

        int numberOfMsgs = 
            Int32.TryParse(Request.Query["no"], out numberOfMsgs)
            ? numberOfMsgs : 100;

        string sql = @"SELECT message.*, user.* FROM message, user
        WHERE message.flagged = 0 AND message.author_id = user.user_id
        ORDER BY message.pub_date DESC LIMIT @num";

        return Ok( GetMsgs(sql, numberOfMsgs) );
    }

    [HttpGet]
    [Route("~/[controller]/msgs/{username}")]
    public IActionResult MsgsUser(string username)
    {
        updateLatest(Request);

        if(!IsRequestFromSimulator(Request))
        {
            return RequestNotFromSimulatorResponse;
        }

        var userId = GetUserId(username);
        if(userId is null) return NotFound();
        
        int numberOfMsgs = 
            Int32.TryParse(Request.Query["no"], out numberOfMsgs)
            ? numberOfMsgs : 100;

        string sql = @"SELECT message.*, user.* FROM message, user 
                   WHERE message.flagged = 0 AND
                   user.user_id = message.author_id AND user.user_id = ?
                   ORDER BY message.pub_date DESC LIMIT @num";
        return Ok( GetMsgs(sql, numberOfMsgs) );
    }

    [HttpPost("~/[controller]/msgs/{username}")]
    public IActionResult MsgsCreateAs(string username, CreateFilteredMsg toCreate)
    {
        updateLatest(Request);
        if(!IsRequestFromSimulator(Request)) return RequestNotFromSimulatorResponse;

        var userId = GetUserId(username);
        if(userId is null) return NotFound();

        var sql = @"INSERT INTO message (author_id, text, pub_date, flagged)
                   VALUES (@aid, @c, @t, 0)";
        
        var sqlCmd = _conn.CreateCommand();
        sqlCmd.CommandText = sql;
        sqlCmd.Parameters.AddWithValue("@aid", userId);
        sqlCmd.Parameters.AddWithValue("@c",toCreate.content);
        sqlCmd.Parameters.AddWithValue("@t",TimeSince1970);
        sqlCmd.ExecuteNonQuery();
        
        return NoContent();
    }

    //Little confusing endpoint, it returns the names of people
    // the username follows.
    [HttpGet("{username}")]
    public IActionResult Fllws(string username)
    {
        updateLatest(Request);

        //if(!IsRequestFromSimulator(Request)) return RequestNotFromSimulatorResponse;

        var userId = GetUserId(username);
        if(userId is null) return NotFound();

        int numberOfFllws = 
            Int32.TryParse(Request.Query["no"], out numberOfFllws)
            ? numberOfFllws : 100;
        
        string sql = @"SELECT user.username FROM user
                   INNER JOIN follower ON follower.whom_id=user.user_id
                   WHERE follower.who_id=@who
                   LIMIT @num";
        var sqlCmd = _conn.CreateCommand();
        sqlCmd.CommandText = sql;
        sqlCmd.Parameters.AddWithValue("@who", userId);
        sqlCmd.Parameters.AddWithValue("@num", numberOfFllws);
        var reader = sqlCmd.ExecuteReader();

        var followerNames = new List<string>();
        while(reader.Read())
        {
            followerNames.Add(reader.GetString(0));
        }

        return Ok(followerNames);
    }

    //this username is different from the Get method
        //this username is the one who we are acting as.
        //so the username is (un)following someone else.
    [HttpPost("~/[controller]/fllws/{username}")]
    public IActionResult FllowOrUnfollowAsUser(string username, FollowOrUnFollowReq fReq)
    {
        updateLatest(Request);

        //if(!IsRequestFromSimulator(Request)) return RequestNotFromSimulatorResponse;

        var userId = GetUserId(username);
        if(userId is null) return NotFound();

        string sql;
        int whomId;
        if(fReq.follow is not null)
        {
            var followsUsername = fReq.follow;
            var followsUserId = GetUserId(followsUsername);
            if(followsUserId is null) return NotFound();

            sql = @"INSERT INTO follower (who_id, whom_id) VALUES (@who, @whom)";
            whomId = followsUserId.Value;
        }
        else //then it is an unfollow request
        {
            var followsUserId = GetUserId(fReq.unfollow!);
            if(followsUserId is null) return NotFound();
            
            sql = @"DELETE FROM follower WHERE who_id=@who and WHOM_ID=@whom";
            whomId = followsUserId.Value;
        }
        var sqlCmd = _conn.CreateCommand();
        sqlCmd.CommandText = sql;
        sqlCmd.Parameters.AddWithValue("@who", userId);
        sqlCmd.Parameters.AddWithValue("@whom", whomId);
        sqlCmd.ExecuteNonQuery();

        return NoContent();
    }

    private List<FileteredMsg> GetMsgs(string sql, int numberOfMsgs)
    {
        var sqlCmd = _conn.CreateCommand();
        sqlCmd.CommandText = sql;
        sqlCmd.Parameters.AddWithValue("@num",numberOfMsgs);
        var reader = sqlCmd.ExecuteReader();
        
        var filteredMsgs = new List<FileteredMsg>();
        while(reader.Read())
        {
            filteredMsgs.Add(new FileteredMsg
            (
                reader.GetString(2),
                reader.GetInt32(3),
                reader.GetString(6)
            ));
        }
        return filteredMsgs;
    }

    public record RegisterInfo(string? username, string? email, string? pwd);
    public record FileteredMsg(string content, int pub_date, string user);
    public record CreateFilteredMsg(string content);
    public record FollowOrUnFollowReq(string? follow, string? unfollow);
    public record LatestInfo(int latest);
}
