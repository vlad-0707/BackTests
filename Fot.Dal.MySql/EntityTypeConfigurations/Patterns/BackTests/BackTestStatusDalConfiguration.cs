using Fot.Common.Patterns;
using Fot.Dal.Models.Patterns.BackTests;

namespace Fot.Dal.MySql.EntityTypeConfigurations.Patterns.BackTests;

public class BackTestStatusDalConfiguration
	: IEntityTypeConfiguration<BackTestStatusDal>
{
	public void Configure(EntityTypeBuilder<BackTestStatusDal> entity)
	{
		entity.ToTable("BackTestStatuses");
		entity.HasKey(x => x.Id);

		#region Properties

		entity.Property(x => x.Id)
			.IsRequired()
			.HasColumnType("int(11)");

		entity.Property(e => e.Name)
			.IsRequired()
			.HasColumnType("varchar(100)");

		#endregion

		entity.HasData(
			new BackTestStatusDal{ Id = BackTestStatusEnum.Unknown, Name = "Неизвестно"},
			new BackTestStatusDal{ Id = BackTestStatusEnum.New, Name = "Новый"},
			new BackTestStatusDal{ Id = BackTestStatusEnum.Running, Name = "Запущен"},
			new BackTestStatusDal{ Id = BackTestStatusEnum.Wait, Name = "Ожидание"},
			new BackTestStatusDal{ Id = BackTestStatusEnum.Stopped, Name = "Остановлен"},
			new BackTestStatusDal{ Id = BackTestStatusEnum.Error, Name = "Ошибка"},
			new BackTestStatusDal{ Id = BackTestStatusEnum.Finish, Name = "Завершён"},
			new BackTestStatusDal{ Id = BackTestStatusEnum.Dropped, Name = "Сброшен"}
		);
	}
}
