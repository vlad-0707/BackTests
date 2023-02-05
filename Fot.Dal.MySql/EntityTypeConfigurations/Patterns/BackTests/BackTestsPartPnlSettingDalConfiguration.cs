using Fot.Dal.Models.Patterns.BackTests;

namespace Fot.Dal.MySql.EntityTypeConfigurations.Patterns.BackTests;

public class BackTestsPartPnlSettingDalConfiguration : IEntityTypeConfiguration<BackTestsPartPnlSettingDal>
{
	public void Configure(EntityTypeBuilder<BackTestsPartPnlSettingDal> entity)
	{
		entity.ToTable("BackTestsPartPnlSettings");
		entity.HasKey(x => x.Id);

		#region Properties

		entity.Property(x => x.Id)
			.IsRequired()
			.HasColumnType("int(11)");

		entity.Property(x => x.BackTestPartId)
			.IsRequired()
			.HasColumnType("int(11)");

		entity.Property(x => x.CheckAfterMilliseconds)
			.HasColumnType("bigint");

		#endregion

		#region Indexes

		entity.HasIndex(x => x.BackTestPartId)
			.HasDatabaseName("BackTestsPartPnlSettings_BackTestParts_Id_fk");

		#endregion

		#region References

		entity.HasOne(x => x.Part)
			.WithMany(x => x.PnlSettings)
			.HasForeignKey(x => x.BackTestPartId)
			.HasPrincipalKey(x => x.Id)
			.OnDelete(DeleteBehavior.Restrict)
			.HasConstraintName("BackTestsPartPnlSettings_BackTestParts_Id_fk");
		#endregion
	}
}
