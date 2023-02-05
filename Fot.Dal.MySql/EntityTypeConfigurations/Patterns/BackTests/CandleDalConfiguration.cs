using Fot.Dal.Models.Markets;

namespace Fot.Dal.MySql.EntityTypeConfigurations.Patterns.BackTests;

public class CandleDalConfiguration : IEntityTypeConfiguration<CandleDal>
{
	public void Configure(EntityTypeBuilder<CandleDal> entity)
	{
		entity.ToTable("Candles");
		entity.HasKey(x => x.Id);

		#region Properties

		entity.Property(x => x.Id).HasColumnType("bigint");

		entity.Property(x => x.SymbolId)
			.IsRequired()
			.HasColumnType("int(11)");

		entity.Property(x => x.TimeFrameId)
			.IsRequired()
			.HasColumnType("int(11)");

		entity.Property(x => x.Timestamp)
			.IsRequired()
			.HasColumnType("bigint");

		entity.Property(x => x.Open)
			.IsRequired()
			.HasColumnType("decimal(28,14)");

		entity.Property(x => x.High)
			.IsRequired()
			.HasColumnType("decimal(28,14)");

		entity.Property(x => x.Low)
			.IsRequired()
			.HasColumnType("decimal(28,14)");

		entity.Property(x => x.Close)
			.IsRequired()
			.HasColumnType("decimal(28,14)");

		entity.Property(x => x.Volume)
			.IsRequired()
			.HasColumnType("decimal(40,14)");

		#endregion

		#region Indexes

		entity.HasIndex(x => x.SymbolId)
			.HasDatabaseName("Candles_Symbols_Id_fk");

		entity.HasIndex(x => x.TimeFrameId)
			.HasDatabaseName("Candles_TimeFrames_Id_fk");

		entity.HasIndex(x => x.Timestamp)
			.HasDatabaseName("Candles_Timestamp_index");

		#endregion

		#region References

		entity.HasOne(x => x.Symbol)
			.WithMany()
			.HasForeignKey(x => x.SymbolId)
			.HasPrincipalKey(x => x.Id)
			.OnDelete(DeleteBehavior.Restrict)
			.HasConstraintName("Candles_Symbols_Id_fk");

		entity.HasOne(x => x.TimeFrame)
			.WithMany()
			.HasForeignKey(x => x.TimeFrameId)
			.HasPrincipalKey(x => x.Id)
			.OnDelete(DeleteBehavior.Restrict)
			.HasConstraintName("Candles_TimeFrames_Id_fk");

		#endregion
	}
}

