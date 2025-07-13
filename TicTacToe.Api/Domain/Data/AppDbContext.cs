using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using TicTacToe.Api.Domain.Models;

namespace TicTacToe.Api.Domain.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<GameSession> GameSessions { get; set; }
        public DbSet<GameRules> GameRules { get; set; }
        public DbSet<GameBoard> GameBoards { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Include
            };

            modelBuilder.Entity<GameSession>(e =>
            {
                e.Property(gs => gs.Winner).HasDefaultValue(null);
                e.Property(gs => gs.CurrMove).HasDefaultValue('x');
                e.Property(gs => gs.Version).IsConcurrencyToken();
            });
            modelBuilder.Entity<GameSession>()
                .HasOne(gs => gs.GameRules)
                .WithOne()
                .HasForeignKey<GameSession>(gs => gs.GameRulesId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<GameSession>()
                .HasOne(gs => gs.GameBoard)
                .WithOne()
                .HasForeignKey<GameSession>(gs => gs.GameBoardId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<GameBoard>()
                .Property(gb => gb.Board)
                .HasConversion(
                    b => JsonConvert.SerializeObject(b, settings),
                    b => JsonConvert.DeserializeObject<char?[,]>(b));

            modelBuilder.Entity<GameRules>(e =>
            {
                e.Property(e => e.Probability).HasDefaultValue(10);
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
