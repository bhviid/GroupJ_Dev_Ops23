using Microsoft.AspNetCore.Mvc;
using MiniTwit.Shared;

namespace MiniTwit.Server;

[ApiController]
[Route("[controller]/[action]")]
public class SlimTwitController : ControllerBase
{
    TwitContext _db;
    DateTime startTime1970 = new (1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
    public static int _latest;
    
    private int TimeSince1970 => (int)(DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;

    private IActionResult RequestNotFromSimulatorResponse => 
            Problem(
                detail: "You are not authorized to use this resource!",
                statusCode: StatusCodes.Status403Forbidden
            );

    public SlimTwitController(TwitContext db)
    {
        _db = db;
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
    }

    //Code duplication
    private int? GetUserId(string username)
    {
        var res = (from u in _db.Users
            where u.Username == username
            select u).FirstOrDefault();
            
        return res == null ? null : res.UserId;
    }

    [HttpGet]
    public IActionResult Latest()
    {
        return Ok(new LatestInfo(_latest));
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterInfo userInfo)
    {
        updateLatest(Request);

        string? err = null;

        Console.WriteLine(userInfo);

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

        await _db.AddAsync(new User { UserId = 0, Username = userInfo.username!, Email = userInfo.email!, Password = userInfo.pwd! });
        await _db.SaveChangesAsync();

        Console.WriteLine("SQL ran");

        return NoContent();
    }

    [HttpGet]
    public IActionResult Msgs()
    {
        updateLatest(Request);

        if(!IsRequestFromSimulator(Request))
        {
            return RequestNotFromSimulatorResponse;
        }

        int numberOfMsgs = 
            Int32.TryParse(Request.Query["no"], out numberOfMsgs)
            ? numberOfMsgs : 100;

        var msgs = (from m in _db.Messages
            join u in _db.Users
                on m.AuthorId equals u.UserId
            where m.Flagged == 0
            orderby m.PubDate descending
                select new MsgDataPair(m, new Author(u.UserId, u.Username, u.Email, null))).Take(numberOfMsgs);
            var filteredMsgs = FilterMsgs(msgs);

        return Ok( filteredMsgs );
    }

    public IEnumerable<FileteredMsg> FilterMsgs(IQueryable<MsgDataPair> msgs)
    {
        foreach (var msgDataPair in msgs)
        {
            yield return new FileteredMsg(
                msgDataPair.Msg.Text,
                DateToInt(msgDataPair.Msg.PubDate),
                msgDataPair.Author.ToString());
        }
    }

    int DateToInt(DateTime? dt)
    {
        return dt == null
            ? (int)(DateTime.Now - startTime1970).TotalSeconds
            : (int)(dt.GetValueOrDefault() - startTime1970).TotalSeconds;
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

        var msgs = (from m in _db.Messages
            join u in _db.Users on m.AuthorId equals u.UserId
            where u.UserId == userId && m.Flagged == 0
            orderby m.PubDate descending
            select new MsgDataPair(m, new Author(u.UserId, u.Username, u.Email, null))).Take(numberOfMsgs);
        return Ok( FilterMsgs(msgs) );
    }

    [HttpPost("~/[controller]/msgs/{username}")]
    public async  Task<IActionResult> MsgsCreateAs(string username, CreateFilteredMsg toCreate)
    {
        updateLatest(Request);
        if(!IsRequestFromSimulator(Request)) return RequestNotFromSimulatorResponse;

        var userId = GetUserId(username);
        if(userId is null) return NotFound();

        await _db.Messages.AddAsync(new Message{AuthorId = userId.Value, Flagged = 0, MessageId = 0, PubDate = DateTime.Now, Text = toCreate.content});
        await _db.SaveChangesAsync();
        
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
        
        
        
        var followers = (from u in _db.Users
        join f in _db.Followings on u.UserId equals f.whom_id
        where f.who_id == userId
        select u.Username).Take(numberOfFllws);

        return Ok(followers);
    }

    //this username is different from the Get method
        //this username is the one who we are acting as.
        //so the username is (un)following someone else.
    [HttpPost("~/[controller]/fllws/{username}")]
    public async Task<IActionResult> FllowOrUnfollowAsUser(string username, FollowOrUnFollowReq fReq)
    {
        updateLatest(Request);

        //if(!IsRequestFromSimulator(Request)) return RequestNotFromSimulatorResponse;

        var userId = GetUserId(username);
        if(userId is null) return NotFound();

        // string sql;
        
        if(fReq.follow is not null)
        {
            var followsUsername = fReq.follow;
            var followsUserId = GetUserId(followsUsername);
            if(followsUserId is null) return NotFound();

            await _db.Followings.AddAsync(new Follows{who_id = userId.Value, whom_id = followsUserId.Value});
        }
        else //then it is an unfollow request
        {
            var followsUserId = GetUserId(fReq.unfollow!);
            if(followsUserId is null) return NotFound();
            await _db.Followings.AddAsync(new Follows{who_id = userId.Value, whom_id = followsUserId.Value});
        }

        await _db.SaveChangesAsync();
        return NoContent();
    }

    public record RegisterInfo(string? username, string? email, string? pwd);
    public record FileteredMsg(string content, int pub_date, string user);
    public record CreateFilteredMsg(string content);
    public record FollowOrUnFollowReq(string? follow, string? unfollow);
    public record LatestInfo(int latest);
}
