namespace Fot.Common.Patterns;

public enum BackTestStatusEnum
{
	Unknown = 0,
	New = 1, //Только создан
	Running = 2, //Выполняется
	Wait = 3, //Ожидает в очереди на запуск
	Stopped = 4, //Остановил человек
	Error = 5, //Закончился c ошибкой
	Finish = 6, //Закончился успешно
	Dropped = 7, // Был уже запущен, но "обнулили результаты"
}
