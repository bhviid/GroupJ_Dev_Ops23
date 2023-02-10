using Microsoft.AspNetCore.Mvc;
using System.Data.SQLite;
using MiniTwit.Shared;
using MiniTwit.Shared.Core;

namespace MiniTwit.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class MiniTwitController : ControllerBase
{
    SQLiteConnection sqliteConn = CreateConnection();

    static SQLiteConnection CreateConnection()
    {
        var sqliteConn = new SQLiteConnection("DataSource=tmp/minitwit.db;Version=3;");
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

    public MiniTwitController(ILogger<MiniTwitController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public IEnumerable<Message> GetAllMessages()
    {
        throw new NotImplementedException();
    }

    [HttpPost]
    [Route(("/register"))]
    [Consumes("application/json")]
    public string Register()
    {

        return "You were successfully registered and can login now";
    }
}
