using Fot.Dal.Models.Patterns.BackTests;

namespace Fot.Dal.MySql.EntityTypeConfigurations.Patterns.BackTests;

public class BackTestResultDalConfiguration
	: IEntityTypeConfiguration<BackTestResultDal>
{
	public void Configure(EntityTypeBuilder<BackTestResultDal> entity)
	{
		entity.ToTable("BackTestResults");
		entity.HasKey(x => x.Id);

		#region Properties

		entity.Property(x => x.Id)
			.IsRequired()
			.HasColumnType("int(11)");

		entity.Property(x => x.BackTestPartId)
			.IsRequired()
			.HasColumnType("int(11)");

		entity.Property(x => x.StartTimestamp)
			.IsRequired()
			.HasColumnType("bigint");

		entity.Property(x => x.EndTimestamp)
			.IsRequired()
			.HasColumnType("bigint");

		entity.Property(x => x.StartOpen)
			.HasColumnType("decimal(28,14)");

		entity.Property(x => x.StartHigh)
			.HasColumnType("decimal(28,14)");

		entity.Property(x => x.StartLow)
			.HasColumnType("decimal(28,14)");

		entity.Property(x => x.StartClose)
			.HasColumnType("decimal(28,14)");

		entity.Property(x => x.StartVolume)
			.HasColumnType("decimal(40,14)");

		entity.Property(x => x.EndOpen)
			.HasColumnType("decimal(28,14)");

		entity.Property(x => x.EndHigh)
			.HasColumnType("decimal(28,14)");

		entity.Property(x => x.EndLow)
			.HasColumnType("decimal(28,14)");

		entity.Property(x => x.EndClose)
			.HasColumnType("decimal(28,14)");

		entity.Property(x => x.EndVolume)
			.HasColumnType("decimal(40,14)");

		entity.Property(x => x.StopBaseCandleTimestamp)
			.HasColumnType("bigint");

		entity.Property(x => x.StopBaseCandleOpen)
			.HasColumnType("decimal(28,14)");

		entity.Property(x => x.StopBaseCandleHigh)
			.HasColumnType("decimal(28,14)");

		entity.Property(x => x.StopBaseCandleLow)
			.HasColumnType("decimal(28,14)");

		entity.Property(x => x.StopBaseCandleClose)
			.HasColumnType("decimal(28,14)");

		entity.Property(x => x.StopBaseCandleVolume)
			.HasColumnType("decimal(40,14)");

		#endregion

		#region Indexes

		entity.HasIndex(x => x.BackTestPartId)
			.HasDatabaseName("BackTestResults_BackTestParts_Id_fk");

		#endregion

		#region References

		entity.HasOne(x => x.BackTestPart)
			.WithMany(x => x.Results)
			.HasForeignKey(x => x.BackTestPartId)
			.HasPrincipalKey(x => x.Id)
			.OnDelete(DeleteBehavior.Restrict)
			.HasConstraintName("BackTestResults_BackTestParts_Id_fk");

		#endregion
	}
}
