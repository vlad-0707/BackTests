using Fot.Dal.Models.Patterns.BackTests;

namespace Fot.Dal.MySql.EntityTypeConfigurations.Patterns.BackTests;

public class BackTestDalConfiguration : IEntityTypeConfiguration<BackTestDal>
{
	public void Configure(EntityTypeBuilder<BackTestDal> entity)
	{
		entity.ToTable("BackTests");
		entity.HasKey(x => x.Id);

		#region Properties

		entity.Property(x => x.Id)
			.IsRequired()
			.HasColumnType("int(11)");

		entity.Property(x => x.PatternId)
			.IsRequired()
			.HasColumnType("int(11)");

		entity.Property(x => x.UserId)
			.IsRequired()
			.HasColumnType("int(11)");

		entity.Property(x => x.StatusId)
			.IsRequired()
			.HasColumnType("int(11)");

		entity.Property(x => x.Since)
			.HasColumnType("timestamp");

		entity.Property(x => x.Until)
			.HasColumnType("timestamp");

		entity.Property(x => x.StartTime)
			.HasColumnType("timestamp");

		entity.Property(x => x.CreatedAt)
			.IsRequired()
			.HasColumnType("timestamp")
			.ValueGeneratedOnAdd();

		entity.Property(x => x.UpdatedAt)
			.IsRequired()
			.HasColumnType("timestamp")
			.ValueGeneratedOnAddOrUpdate();

		#endregion

		#region Indexes

		entity.HasIndex(x => x.PatternId)
			.HasDatabaseName("BackTests_Patterns_Id_fk");

		entity.HasIndex(x => x.UserId)
			.HasDatabaseName("BackTests_AspNetUsers_Id_fk");

		entity.HasIndex(x => x.StatusId)
			.HasDatabaseName("BackTests_BackTestStatuses_Id_fk");

		#endregion

		#region References

		entity.HasOne(x => x.Pattern)
			.WithMany()
			.HasForeignKey(x => x.PatternId)
			.HasPrincipalKey(x => x.Id)
			.OnDelete(DeleteBehavior.Restrict)
			.HasConstraintName("BackTests_Patterns_Id_fk");

		entity.HasOne(x => x.User)
			.WithMany()
			.HasForeignKey(x => x.UserId)
			.HasPrincipalKey(x => x.Id)
			.OnDelete(DeleteBehavior.Restrict)
			.HasConstraintName("BackTests_AspNetUsers_Id_fk");

		entity.HasOne(x => x.Status)
			.WithMany()
			.HasForeignKey(x => x.StatusId)
			.HasPrincipalKey(x => x.Id)
			.OnDelete(DeleteBehavior.Restrict)
			.HasConstraintName("BackTests_BackTestStatuses_Id_fk");

		#endregion
	}
}
