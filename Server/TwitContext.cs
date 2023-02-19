namespace MiniTwit.Server;

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using MiniTwit.Shared;

public class TwitContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Follows> Followings { get; set; }
    public DbSet<Message> Messages { get; set; }
      public TwitContext(DbContextOptions<TwitContext> options)
        : base(options)
    {
    }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Message>().HasKey(m => m.MessageId);
        modelBuilder.Entity<Message>().ToTable("message");

        modelBuilder.Entity<User>().HasKey(u => u.UserId);
        modelBuilder.Entity<User>().ToTable("user");

        modelBuilder.Entity<Follows>().HasKey(f => new {f.who_id, f.whom_id});
        modelBuilder.Entity<Follows>().ToTable("follower");

    }
}