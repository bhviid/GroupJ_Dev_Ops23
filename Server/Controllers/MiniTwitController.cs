using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniTwit.Shared;
using Serilog;

namespace MiniTwit.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class MiniTwitController : ControllerBase
{
    private readonly TwitContext _db;
    private static DateTime Jan1970 = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
    private readonly int _perPage = 30;

    public MiniTwitController(TwitContext db)
    {
        _db = db;
    }

    [HttpGet]
    public IActionResult GetAllMessages()
    {
        var (startIndex, pageSize) = GetStartIndexAndPageSizeOrDefaults(Request);
        // var result = _db.Messages.Include(m => m.User)
        // .OrderByDescending(m => m.PubDate)
        // .Where(m => m.Flagged == 0)
        // .Select(m => new  Author(m.User.UserId,m.User.Username, m.Text, ""));
        var result = (from m in _db.Messages
                      join u in _db.Users on m.AuthorId equals u.UserId
                      where m.Flagged == 0
                      orderby m.PubDate descending
                      select new MsgDataPair(m, new Author(u.UserId, u.Username, u.Email,
                                                GravatarUrlStringFromEmail(u.Email))
                       ));
        var total = result.Count();
        var res = result.Skip(startIndex)
                        .Take(pageSize);
       
		Log.Information("Retrieved {Count} messages for all users with pagination startIndex {StartIndex} and pageSize {PageSize}", total, startIndex, pageSize);

        return Ok(res);
        //return Ok(new MsgDataAndLength(total, res));
    }

    [HttpGet]
    [Route("/minitwit/feed/{username}")]
    public IActionResult GetUserFeed(string username)
    {
        //the user who's feed we would like to get it.
        var userId = GetUserId(username);
        Console.WriteLine($" {userId}, {username}");
        if(userId is null)
        {
            return NotFound();
        }
        var (startIndex, pageSize) = GetStartIndexAndPageSizeOrDefaults(Request);

        // Pretty sure, that ToList() forces the query to be executed in memory rather than on db
        //which greatly improves the speed.
        var flws = _db.Followings.Where(f => f.who_id == userId).Select(f => f.whom_id).ToList();

        var result = (from m in _db.Messages
                      join u in _db.Users on m.AuthorId equals u.UserId
                      where m.Flagged == 0 && (
                          u.UserId == userId || flws.Contains(u.UserId)
                      )
                      orderby m.PubDate descending
                      select new MsgDataPair(m, new Author(u.UserId, u.Username, u.Email,
                                              GravatarUrlStringFromEmail(u.Email))
                      ));
        var total = result.Count();
        var res = result.Skip(startIndex)
                        .Take(pageSize);
        
		Log.Information("Retrieved {AmountOfTweets} messages for user {@Username}'s feed with startIndex {StartIndex} and pageSize {PageSize}", total, username, startIndex, pageSize);
             
        return Ok( new MsgDataAndLength(total, res) );
    }

    [HttpGet("is-follower/{whoUsername}/{whomUsername}")]
    public async Task<IActionResult> IsFollower(string whoUsername, string whomUsername)
    {
        int? whoId = GetUserId(whoUsername);
        int? whomId = GetUserId(whomUsername);

        if(whoId is null || whomId is null)
        {
			Log.Information("User not found when checking follower status for {@WhoUsername} and {@WhomUsername}", whoUsername, whomUsername);
            return StatusCode(StatusCodes.Status404NotFound);
        }

        var res = _db.Followings.Where(f => f.who_id == whoId && f.whom_id == whomId).Select(f => f).FirstOrDefault();
		
        return res is not null ? Ok(true) : Ok(false);
    }

    [HttpGet]
    [Route("/minitwit/{username}")]
    public IActionResult GetUserTimeline(string username)
    {
        var user = (from u in _db.Users where u.Username == username select u).FirstOrDefault();
        if (user is null)
        {
            Log.Information("User not found when getting timeline for {Username}", username);
            return NotFound();
        }
        var (startIndex, pageSize) = GetStartIndexAndPageSizeOrDefaults(Request);
        
        var author = new Author(user.UserId, user.Username, user.Email, GravatarUrlStringFromEmail(user.Email));
        var timeline = _db.Messages.Where(m => m.AuthorId == user.UserId)
                                    .OrderByDescending(m => m.PubDate)
                                    .Select(m => new MsgDataPair(m,author));
        var total = timeline.Count();
        var t = timeline.Skip(startIndex)
                        .Take(pageSize);
        return Ok( new MsgDataAndLength(total, t) );
    }

    [HttpPost]
    [Route("{username}/follow")]
    [Consumes("application/json")]
    public async Task<IActionResult> Follow(string username, User activeUser)
    {
        var whomId = GetUserId(username);
        var activeUserId = GetUserId(activeUser.Username);
        if (whomId is null)
        {
            Log.Information("{@User} tried to follow {Non-existing-Username} but the user is not registeret", activeUser, username);
            return NotFound();
        }
        //Should never happen tbh, since we know a logged in user has a username in the frontend.
        if (activeUserId is null)
        {
            Log.Warning("Active user is null when trying to follow another user");
            return BadRequest();
        }
        
        await _db.Followings.AddAsync(new Follows { who_id = activeUserId.Value, whom_id = whomId.Value });
        await _db.SaveChangesAsync();
        Log.Information("{@User} is now following {@Username}", activeUser, username);
        return Ok($"You are now following {username}");
    }

    [HttpPost]
    [Route("{username}/unfollow")]
    [Consumes("application/json")]
    public IActionResult UnFollow(string username, User activeUser)
    {
        var whomId = GetUserId(username);
        if (whomId is null) 
		{
			Log.Information("User {Username} not found", username);
			return NotFound();
		}
        var activeUserId = GetUserId(activeUser.Username) ?? 0;
		
        var toRemove = (from f in _db.Followings
                        where f.who_id == activeUserId && f.whom_id == whomId
                        select f).FirstOrDefault();
        if (toRemove is null)
        {
			Log.Information("{@ActiveUser} is not following {@Username}", activeUser, username);
            return NotFound("You are not following this user");
        }

        _db.Followings.Remove(toRemove);
        _db.SaveChanges();
		
		Log.Information("{@ActiveUser} has successfully unfollowed {@Username}", activeUser, username);

        return Ok($"You are no longer following {username}");
    }

    [HttpPost]
    [Route("add-message")]
    [Consumes("application/json")]
    public async Task<IActionResult> AddMessage(MessageCreateDTO message)
    {
        var authorId = GetUserId(message.Author);
        if(authorId is null)
        {
			Log.Information("{@ActiveUser} not found", message.Author);
            return BadRequest();
        }

        await _db.Messages.AddAsync(new Message
        {
            MessageId = 0,
            AuthorId = authorId.Value,
            Text = message.Text,
            PubDate = DateTime.Now,
            Flagged = 0
        });
        await _db.SaveChangesAsync();
    	
		Log.Information("@{ActiveUser} Tweetet: {@Text}",authorId, message.Text);

        return Ok($"Message posted: {message.Text}");
    }
    
    private int? GetUserId(string username)
    {
        var s = _db.Users.Where(u => u.Username == username).FirstOrDefault();
        return s is not null ? s.UserId : null;
    }

    [HttpGet]
    [Route("md5/{email}/{size}")]
    public string Md5HashEmailForGravatarString(string email, int size = 48)
    {
        //Must be here since MD5 is disabled in blazor wasm...
        using var md5 = System.Security.Cryptography.MD5.Create();
        byte[] md5ed = md5.ComputeHash(System.Text.Encoding.ASCII.GetBytes(email.Trim().ToLower()));
        return $"http://www.gravatar.com/avatar/{Convert.ToHexString(md5ed).ToLower()}?d=identicon&s={size}";
    }

    [HttpPost]
    [Route(("register"))]
    [Consumes("application/json")]
    public async Task<IActionResult> Register(UserCreateDTO user)
    {
        
        var PwHash = Md5HashPassword(user.Password);

        if (user.Username == "")
        {
            Log.Information("Invalid Username");
            return BadRequest("Invalid Username");
        }
        if (user.Password == "")
        {
            Log.Information("Password cannot be empty!");
            return BadRequest("Password cannot be empty!");
        }
        if (user.Password != user.PasswordRepeat)
        {
            Log.Information("Passwords don't match");
            return BadRequest("Passwords don't match");
        }
        if (!Utility_Methods.IsValidEmail(user.Email))
        {
            Log.Information("Invalid E-mail");
            return BadRequest("Invalid E-mail");
        }
        if (UserExists(new UserDTO(user.Username, user.Email, user.Password)))
        {
            Log.Information("User already exists");
            return Conflict("User already exists");
        }

        await _db.Users.AddAsync(new User
        {
            UserId = 0,
            Username = user.Username,
            Password = PwHash,
            Email = user.Email
        });
        await _db.SaveChangesAsync();
        var createdUser = GetUser(user.Username);

        if (createdUser is not null)
        {
            Log.Information("{@User} has joined",createdUser) ;
            return Created($"user/{createdUser.UserId}", createdUser);
        }
        Log.Information("Nothing was changed");

        return BadRequest("Nothing was changed");
    }

    public User? GetUser(string username)
    {
        var user = (from u in _db.Users
                    where u.Username == username
                    select u).FirstOrDefault();
        return user;
    }

    private bool UserExists(UserDTO user)
    {
        return GetUser(user.Username) is not null;
    }

    [HttpPost("login")]
    public IActionResult Login(UserLoginDTO loginData)
    {
        var user = _db.Users.Where(u => u.Username == loginData.Username).FirstOrDefault();

        if(user is null)
        {
            Log.Information("Invalid username {@Username} attempted to log in", loginData.Username);
            return StatusCode(StatusCodes.Status401Unauthorized, "Invalid username");
        }

        // hash the password from Post/request
        var PwHash = Md5HashPassword(loginData.Password);

        //check if the hash from db matches the hash from post/request.
        if (user.Password != PwHash)
        {
            Log.Information("{@User} tried to login with the wrong password", user);
            return StatusCode(StatusCodes.Status401Unauthorized, "Invalid password");
        }
        Log.Information("{@User} has logged in", user);
        return Ok(new UserDTO(user.Username, user.Email, user.Password));
    }

    private string Md5HashPassword(string rawPass) 
    {
        using var md5 = System.Security.Cryptography.MD5.Create();
        var md5ed = md5.ComputeHash(System.Text.Encoding.ASCII.GetBytes(rawPass));
        return System.Text.Encoding.UTF8.GetString(md5ed);
    }

    private static String GravatarUrlStringFromEmail(string email, int size)
    {
        using var md5 = System.Security.Cryptography.MD5.Create();
        byte[] md5ed = md5.ComputeHash(System.Text.Encoding.ASCII.GetBytes(email.Trim().ToLower()));
        return $"http://www.gravatar.com/avatar/{Convert.ToHexString(md5ed).ToLower()}?d=identicon&s={size}";
    }

    private static String GravatarUrlStringFromEmail(string email) => GravatarUrlStringFromEmail(email, 48);

    private (int,int) GetStartIndexAndPageSizeOrDefaults(HttpRequest req)
    {
        int startIndex = int.TryParse(Request.Query["startIndex"], out startIndex) ? startIndex : 0;
        int pageSize = int.TryParse(Request.Query["pageSize"], out pageSize) ? pageSize : _perPage;

        return (startIndex,pageSize);
    }
}
