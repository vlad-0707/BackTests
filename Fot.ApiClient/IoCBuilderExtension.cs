#nullable disable
using Autofac;
using Fot.BLL.Configs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Fot.ApiClient;

public static class IoCBuilderExtension
{
	public static IServiceCollection ConfigureFotApiClient(this IServiceCollection services, IConfiguration configuration)
	{
		services.Configure<FotApiConfig>(configuration.GetSection(nameof(FotApiConfig)));
		return services;
	}

	public static IServiceCollection AddFotApiClient(this IServiceCollection services, IConfiguration configuration)
	{
		return services
			.AddSingleton<FotApiSocketClient>();
	}

	public static IServiceCollection AddAllModulesFotApiClient(this IServiceCollection services, IConfiguration configuration)
	{
		return services
			.ConfigureFotApiClient(configuration)
			.AddFotApiClient(configuration)

			.AddSingleton<ServicesSocketClientModule>();
	}

	public static ContainerBuilder AddFotApiClient(this ContainerBuilder builder)
	{
		builder.RegisterType<FotApiSocketClient>().AsSelf().SingleInstance();
		return builder;
	}

	public static ContainerBuilder AddAllModulesFotApiClient(this ContainerBuilder builder)
	{
		builder.RegisterType<ServicesSocketClientModule>().AsSelf().SingleInstance();

		return builder
			.AddFotApiClient();
	}
}
