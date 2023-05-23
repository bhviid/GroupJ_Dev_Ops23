using Microsoft.EntityFrameworkCore;

namespace MiniTwit.Shared;
public class TwitContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Follows> Followings { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<Latest> Latests { get; set; }
    
      public TwitContext(DbContextOptions<TwitContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Message>().HasKey(m => m.MessageId);

        DateTime startTime1970 = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        modelBuilder.Entity<Message>()
                .Property(m => m.PubDate)
                .HasConversion(
                    c => c == null ? (int) (DateTime.Now - startTime1970).TotalSeconds : (int) (c.GetValueOrDefault() - startTime1970).TotalSeconds,
                    c =>  startTime1970.AddSeconds(c));
                    
        modelBuilder.Entity<Message>().ToTable("message");

        modelBuilder.Entity<User>().HasKey(u => u.UserId);
        modelBuilder.Entity<User>().ToTable("user");

        modelBuilder.Entity<Follows>().HasKey(f => new {f.who_id, f.whom_id});
        modelBuilder.Entity<Follows>().ToTable("follower");

        modelBuilder.Entity<Latest>().HasIndex(l => new {l.CreatedAt, l.Value});

        modelBuilder.Entity<Latest>()
            .Property(l => l.CreatedAt)
                .HasConversion(
                    c =>  (int) (c - startTime1970).TotalSeconds,
                    c =>  startTime1970.AddSeconds(c));
    }
}