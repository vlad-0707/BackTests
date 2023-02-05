using Fot.Bll.Common.Patterns.BackTests;
using Microsoft.Extensions.Configuration;

// ReSharper disable once CheckNamespace
namespace Fot.IoC;

public static class IoC
{
	public static IServiceCollection WebRegisterPatternBackTests<TBackTestTransmitter>(
		this IServiceCollection services,
		IConfiguration configuration)

		where TBackTestTransmitter : class, IBackTestTransmitter
	{

		services.AddScoped<IBackTestTransmitter, TBackTestTransmitter>();
		services.AddScoped<IWebPatternBackTestManager, WebPatternBackTestManager>();

		return services;
	}

	public static IServiceCollection ConfigurePatternBackTests(
		this IServiceCollection services,
		IConfiguration configuration)
	{

		return services;
	}

	public static Autofac.ContainerBuilder ServiceRegisterPatternBackTests(
		this Autofac.ContainerBuilder builder)
	{
		return builder;
	}
}
