using Fot.Dal.Models.Patterns.BackTests;

namespace Fot.Dal.MySql.EntityTypeConfigurations.Patterns.BackTests;

public class BackTestResultPnlDalConfiguration : IEntityTypeConfiguration<BackTestResultPnlDal>
{
	public void Configure(EntityTypeBuilder<BackTestResultPnlDal> entity)
	{
		entity.ToTable("BackTestResultPnls");
		entity.HasKey(x => x.Id);

		#region Properties

		entity.Property(x => x.Id)
			.IsRequired()
			.HasColumnType("int(11)");

		entity.Property(x => x.BackTestResultId)
			.IsRequired()
			.HasColumnType("int(11)");

		entity.Property(x => x.BackTestSettingId)
			.IsRequired()
			.HasColumnType("int(11)");

		entity.Property(x => x.LongResult)
			.IsRequired()
			.HasColumnType("decimal(15,5)");

		entity.Property(x => x.ResultTimestamp)
			.HasColumnType("bigint");

		entity.Property(x => x.ResultOpen)
			.HasColumnType("decimal(28,14)");

		entity.Property(x => x.ResultHigh)
			.HasColumnType("decimal(28,14)");

		entity.Property(x => x.ResultLow)
			.HasColumnType("decimal(28,14)");

		entity.Property(x => x.ResultClose)
			.HasColumnType("decimal(28,14)");

		entity.Property(x => x.ResultVolume)
			.HasColumnType("decimal(40,14)");

		entity.Property(x => x.High)
			.HasColumnType("decimal(28,14)");

		entity.Property(x => x.HighTimestamp)
			.HasColumnType("bigint");

		entity.Property(x => x.Low)
			.HasColumnType("decimal(28,14)");

		entity.Property(x => x.LowTimestamp)
			.HasColumnType("bigint");

		#endregion

		#region Indexes

		entity.HasIndex(x => x.BackTestResultId)
			.HasDatabaseName("BackTestResultPnls_BackTestResults_Id_fk");

		entity.HasIndex(x => x.BackTestSettingId)
			.HasDatabaseName("BackTestResultPnls_BackTestsPartPnlSettings_Id_fk");

		#endregion

		#region References

		entity.HasOne(x => x.Result)
			.WithMany()
			.HasForeignKey(x => x.BackTestResultId)
			.HasPrincipalKey(x => x.Id)
			.OnDelete(DeleteBehavior.Restrict)
			.HasConstraintName("BackTestResultPnls_BackTestResults_Id_fk");

		entity.HasOne(x => x.Setting)
			.WithMany()
			.HasForeignKey(x => x.BackTestSettingId)
			.HasPrincipalKey(x => x.Id)
			.OnDelete(DeleteBehavior.Restrict)
			.HasConstraintName("BackTestResultPnls_BackTestsPartPnlSettings_Id_fk");

		#endregion
	}
}
