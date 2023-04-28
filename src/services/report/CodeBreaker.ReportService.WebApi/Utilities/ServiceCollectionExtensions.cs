namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    #region Singleton
    public static IServiceCollection AddSingleton<TService1, TService2, TImplementation>(this IServiceCollection services)
        where TService1 : class
        where TService2 : class
        where TImplementation : class, TService1, TService2
    {
        services.AddSingleton<TImplementation>();
        services.AddSingleton<TService1>(x => x.GetRequiredService<TImplementation>());
        services.AddSingleton<TService2>(x => x.GetRequiredService<TImplementation>());
        return services;
    }

    public static IServiceCollection AddSingleton<TService1, TService2, TImplementation>(this IServiceCollection services, Func<IServiceProvider, TImplementation> implementationFactory)
        where TService1 : class
        where TService2 : class
        where TImplementation : class, TService1, TService2
    {
        services.AddSingleton(implementationFactory);
        services.AddSingleton<TService1>(x => x.GetRequiredService<TImplementation>());
        services.AddSingleton<TService2>(x => x.GetRequiredService<TImplementation>());
        return services;
    }

    public static IServiceCollection AddSingleton<TService1, TService2, TService3, TImplementation>(this IServiceCollection services)
        where TService1 : class
        where TService2 : class
        where TService3 : class
        where TImplementation : class, TService1, TService2, TService3
    {
        services.AddSingleton<TImplementation>();
        services.AddSingleton<TService1>(x => x.GetRequiredService<TImplementation>());
        services.AddSingleton<TService2>(x => x.GetRequiredService<TImplementation>());
        services.AddSingleton<TService3>(x => x.GetRequiredService<TImplementation>());
        return services;
    }

    public static IServiceCollection AddSingleton<TService1, TService2, TService3, TImplementation>(this IServiceCollection services, Func<IServiceProvider, TImplementation> implementationFactory)
        where TService1 : class
        where TService2 : class
        where TService3 : class
        where TImplementation : class, TService1, TService2, TService3
    {
        services.AddSingleton(implementationFactory);
        services.AddSingleton<TService1>(x => x.GetRequiredService<TImplementation>());
        services.AddSingleton<TService2>(x => x.GetRequiredService<TImplementation>());
        services.AddSingleton<TService3>(x => x.GetRequiredService<TImplementation>());
        return services;
    }

    public static IServiceCollection AddSingleton<TService1, TService2, TService3, TService4, TImplementation>(this IServiceCollection services)
        where TService1 : class
        where TService2 : class
        where TService3 : class
        where TService4 : class
        where TImplementation : class, TService1, TService2, TService3, TService4
    {
        services.AddSingleton<TImplementation>();
        services.AddSingleton<TService1>(x => x.GetRequiredService<TImplementation>());
        services.AddSingleton<TService2>(x => x.GetRequiredService<TImplementation>());
        services.AddSingleton<TService3>(x => x.GetRequiredService<TImplementation>());
        services.AddSingleton<TService4>(x => x.GetRequiredService<TImplementation>());
        return services;
    }

    public static IServiceCollection AddSingleton<TService1, TService2, TService3, TService4, TImplementation>(this IServiceCollection services, Func<IServiceProvider, TImplementation> implementationFactory)
        where TService1 : class
        where TService2 : class
        where TService3 : class
        where TService4 : class
        where TImplementation : class, TService1, TService2, TService3, TService4
    {
        services.AddSingleton(implementationFactory);
        services.AddSingleton<TService1>(x => x.GetRequiredService<TImplementation>());
        services.AddSingleton<TService2>(x => x.GetRequiredService<TImplementation>());
        services.AddSingleton<TService3>(x => x.GetRequiredService<TImplementation>());
        services.AddSingleton<TService4>(x => x.GetRequiredService<TImplementation>());
        return services;
    }

    public static IServiceCollection AddSingleton<TService1, TService2, TService3, TService4, TService5, TImplementation>(this IServiceCollection services)
        where TService1 : class
        where TService2 : class
        where TService3 : class
        where TService4 : class
        where TService5 : class
        where TImplementation : class, TService1, TService2, TService3, TService4, TService5
    {
        services.AddSingleton<TImplementation>();
        services.AddSingleton<TService1>(x => x.GetRequiredService<TImplementation>());
        services.AddSingleton<TService2>(x => x.GetRequiredService<TImplementation>());
        services.AddSingleton<TService3>(x => x.GetRequiredService<TImplementation>());
        services.AddSingleton<TService4>(x => x.GetRequiredService<TImplementation>());
        services.AddSingleton<TService5>(x => x.GetRequiredService<TImplementation>());
        return services;
    }

    public static IServiceCollection AddSingleton<TService1, TService2, TService3, TService4, TService5, TImplementation>(this IServiceCollection services, Func<IServiceProvider, TImplementation> implementationFactory)
        where TService1 : class
        where TService2 : class
        where TService3 : class
        where TService4 : class
        where TService5 : class
        where TImplementation : class, TService1, TService2, TService3, TService4, TService5
    {
        services.AddSingleton(implementationFactory);
        services.AddSingleton<TService1>(x => x.GetRequiredService<TImplementation>());
        services.AddSingleton<TService2>(x => x.GetRequiredService<TImplementation>());
        services.AddSingleton<TService3>(x => x.GetRequiredService<TImplementation>());
        services.AddSingleton<TService4>(x => x.GetRequiredService<TImplementation>());
        services.AddSingleton<TService5>(x => x.GetRequiredService<TImplementation>());
        return services;
    }
    #endregion

    #region Scoped
    public static IServiceCollection AddScoped<TService1, TService2, TImplementation>(this IServiceCollection services)
        where TService1 : class
        where TService2 : class
        where TImplementation : class, TService1, TService2
    {
        services.AddScoped<TImplementation>();
        services.AddScoped<TService1>(x => x.GetRequiredService<TImplementation>());
        services.AddScoped<TService2>(x => x.GetRequiredService<TImplementation>());
        return services;
    }

    public static IServiceCollection AddScoped<TService1, TService2, TImplementation>(this IServiceCollection services, Func<IServiceProvider, TImplementation> implementationFactory)
        where TService1 : class
        where TService2 : class
        where TImplementation : class, TService1, TService2
    {
        services.AddScoped(implementationFactory);
        services.AddScoped<TService1>(x => x.GetRequiredService<TImplementation>());
        services.AddScoped<TService2>(x => x.GetRequiredService<TImplementation>());
        return services;
    }

    public static IServiceCollection AddScoped<TService1, TService2, TService3, TImplementation>(this IServiceCollection services)
        where TService1 : class
        where TService2 : class
        where TService3 : class
        where TImplementation : class, TService1, TService2, TService3
    {
        services.AddScoped<TImplementation>();
        services.AddScoped<TService1>(x => x.GetRequiredService<TImplementation>());
        services.AddScoped<TService2>(x => x.GetRequiredService<TImplementation>());
        services.AddScoped<TService3>(x => x.GetRequiredService<TImplementation>());
        return services;
    }

    public static IServiceCollection AddScoped<TService1, TService2, TService3, TImplementation>(this IServiceCollection services, Func<IServiceProvider, TImplementation> implementationFactory)
        where TService1 : class
        where TService2 : class
        where TService3 : class
        where TImplementation : class, TService1, TService2, TService3
    {
        services.AddScoped(implementationFactory);
        services.AddScoped<TService1>(x => x.GetRequiredService<TImplementation>());
        services.AddScoped<TService2>(x => x.GetRequiredService<TImplementation>());
        services.AddScoped<TService3>(x => x.GetRequiredService<TImplementation>());
        return services;
    }

    public static IServiceCollection AddScoped<TService1, TService2, TService3, TService4, TImplementation>(this IServiceCollection services)
        where TService1 : class
        where TService2 : class
        where TService3 : class
        where TService4 : class
        where TImplementation : class, TService1, TService2, TService3, TService4
    {
        services.AddScoped<TImplementation>();
        services.AddScoped<TService1>(x => x.GetRequiredService<TImplementation>());
        services.AddScoped<TService2>(x => x.GetRequiredService<TImplementation>());
        services.AddScoped<TService3>(x => x.GetRequiredService<TImplementation>());
        services.AddScoped<TService4>(x => x.GetRequiredService<TImplementation>());
        return services;
    }

    public static IServiceCollection AddScoped<TService1, TService2, TService3, TService4, TImplementation>(this IServiceCollection services, Func<IServiceProvider, TImplementation> implementationFactory)
        where TService1 : class
        where TService2 : class
        where TService3 : class
        where TService4 : class
        where TImplementation : class, TService1, TService2, TService3, TService4
    {
        services.AddScoped(implementationFactory);
        services.AddScoped<TService1>(x => x.GetRequiredService<TImplementation>());
        services.AddScoped<TService2>(x => x.GetRequiredService<TImplementation>());
        services.AddScoped<TService3>(x => x.GetRequiredService<TImplementation>());
        services.AddScoped<TService4>(x => x.GetRequiredService<TImplementation>());
        return services;
    }

    public static IServiceCollection AddScoped<TService1, TService2, TService3, TService4, TService5, TImplementation>(this IServiceCollection services)
        where TService1 : class
        where TService2 : class
        where TService3 : class
        where TService4 : class
        where TService5 : class
        where TImplementation : class, TService1, TService2, TService3, TService4, TService5
    {
        services.AddScoped<TImplementation>();
        services.AddScoped<TService1>(x => x.GetRequiredService<TImplementation>());
        services.AddScoped<TService2>(x => x.GetRequiredService<TImplementation>());
        services.AddScoped<TService3>(x => x.GetRequiredService<TImplementation>());
        services.AddScoped<TService4>(x => x.GetRequiredService<TImplementation>());
        services.AddScoped<TService5>(x => x.GetRequiredService<TImplementation>());
        return services;
    }

    public static IServiceCollection AddScoped<TService1, TService2, TService3, TService4, TService5, TImplementation>(this IServiceCollection services, Func<IServiceProvider, TImplementation> implementationFactory)
        where TService1 : class
        where TService2 : class
        where TService3 : class
        where TService4 : class
        where TService5 : class
        where TImplementation : class, TService1, TService2, TService3, TService4, TService5
    {
        services.AddScoped(implementationFactory);
        services.AddScoped<TService1>(x => x.GetRequiredService<TImplementation>());
        services.AddScoped<TService2>(x => x.GetRequiredService<TImplementation>());
        services.AddScoped<TService3>(x => x.GetRequiredService<TImplementation>());
        services.AddScoped<TService4>(x => x.GetRequiredService<TImplementation>());
        services.AddScoped<TService5>(x => x.GetRequiredService<TImplementation>());
        return services;
    }
    #endregion
}
