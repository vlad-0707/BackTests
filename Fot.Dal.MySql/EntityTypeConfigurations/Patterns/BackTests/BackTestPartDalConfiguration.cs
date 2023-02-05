using Fot.Dal.Models.Patterns.BackTests;

namespace Fot.Dal.MySql.EntityTypeConfigurations.Patterns.BackTests;

public class BackTestPartDalConfiguration : IEntityTypeConfiguration<BackTestPartDal>
{
	public void Configure(EntityTypeBuilder<BackTestPartDal> entity)
	{
		entity.ToTable("BackTestParts");
		entity.HasKey(x => x.Id);

		#region Properties

		entity.Property(x => x.Id)
			.IsRequired()
			.HasColumnType("int(11)");

		entity.Property(x => x.BackTestId)
			.IsRequired()
			.HasColumnType("int(11)");

		entity.Property(x => x.SymbolId)
			.IsRequired()
			.HasColumnType("int(11)");

		entity.Property(x => x.TimeFrameId)
			.IsRequired()
			.HasColumnType("int(11)");

		entity.Property(x => x.StatusId)
			.IsRequired()
			.HasColumnType("int(11)");

		entity.Property(x => x.CompletionTime)
			.HasColumnType("timestamp");

		entity.Property(x => x.LastCalcTime)
			.HasColumnType("timestamp");

		entity.Property(x => x.LastCalcTimestamp)
			.HasColumnType("bigint");

		#endregion

		#region Indexes

		entity.HasIndex(x => x.BackTestId)
			.HasDatabaseName("BackTestParts_BackTests_Id_fk");

		entity.HasIndex(x => x.SymbolId)
			.HasDatabaseName("BackTestParts_Symbols_Id_fk");

		entity.HasIndex(x => x.TimeFrameId)
			.HasDatabaseName("BackTestParts_TimeFrames_Id_fk");

		entity.HasIndex(x => x.StatusId)
			.HasDatabaseName("BackTestParts_BackTestStatuses_Id_fk");

		#endregion

		#region References

		entity.HasOne(x => x.BackTest)
			.WithMany(x => x.Parts)
			.HasForeignKey(x => x.BackTestId)
			.HasPrincipalKey(x => x.Id)
			.OnDelete(DeleteBehavior.Restrict)
			.HasConstraintName("BackTestParts_BackTests_Id_fk");

		entity.HasOne(x => x.Symbol)
			.WithMany()
			.HasForeignKey(x => x.SymbolId)
			.HasPrincipalKey(x => x.Id)
			.OnDelete(DeleteBehavior.Restrict)
			.HasConstraintName("BackTestParts_Symbols_Id_fk");

		entity.HasOne(x => x.TimeFrame)
			.WithMany()
			.HasForeignKey(x => x.TimeFrameId)
			.HasPrincipalKey(x => x.Id)
			.OnDelete(DeleteBehavior.Restrict)
			.HasConstraintName("BackTestParts_TimeFrames_Id_fk");

		entity.HasOne(x => x.Status)
			.WithMany()
			.HasForeignKey(x => x.StatusId)
			.HasPrincipalKey(x => x.Id)
			.OnDelete(DeleteBehavior.Restrict)
			.HasConstraintName("BackTestParts_BackTestStatuses_Id_fk");

		#endregion

	}
}
