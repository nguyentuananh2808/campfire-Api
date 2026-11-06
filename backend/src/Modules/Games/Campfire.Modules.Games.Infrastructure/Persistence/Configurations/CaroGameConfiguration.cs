using Campfire.Modules.Games.Domain.Caro;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Campfire.Modules.Games.Infrastructure.Persistence.Configurations;

public sealed class CaroGameConfiguration : IEntityTypeConfiguration<CaroGame>
{
    public void Configure(EntityTypeBuilder<CaroGame> builder)
    {
        builder.ToTable("CaroGames", GamesDbContext.Schema, table =>
        {
            table.HasCheckConstraint("CK_CaroGames_Board", "length(\"Board\") = 9 AND \"Board\" ~ '^[XO-]{9}$'");
            table.HasCheckConstraint("CK_CaroGames_Players", "\"XPlayerUserId\" <> \"OPlayerUserId\"");
        });
        builder.HasKey(game => game.Id);
        builder.Property(game => game.Board).HasMaxLength(9).IsFixedLength().IsRequired();
        builder.Property(game => game.Status).HasConversion<string>().HasMaxLength(8).IsRequired();
        builder.Property(game => game.Version).IsConcurrencyToken();
        builder.Property(game => game.CreatedAtUtc).HasColumnType("timestamp with time zone");
        builder.Property(game => game.UpdatedAtUtc).HasColumnType("timestamp with time zone");
        builder.HasIndex(game => new { game.XPlayerUserId, game.UpdatedAtUtc });
        builder.HasIndex(game => new { game.OPlayerUserId, game.UpdatedAtUtc });
        builder.HasIndex(game => game.GroupId);
    }
}
