using Microsoft.Extensions.Caching.Memory;
using OregonNexus.Broker.Data;
using OregonNexus.Broker.SharedKernel;
using OregonNexus.Broker.Worker;
using OregonNexus.Broker.Worker.Services;

var builder = Host.CreateDefaultBuilder(args);

builder.ConfigureServices((hostContext, services) =>
{
    switch (hostContext.Configuration["DatabaseProvider"])
    {
        case DbProviderType.MsSql:
            services.AddSingleton<BrokerDbContext, MsSqlDbContext>();
            break;

        case DbProviderType.PostgreSql:
            services.AddSingleton<BrokerDbContext, PostgresDbContext>();
            break;
    }

    services.AddSingleton(typeof(EfRepository<>));
    services.AddSingleton(typeof(CachedRepository<>));

    services.AddSingleton(typeof(IRepository<>), typeof(EfRepository<>));
    services.AddSingleton(typeof(IReadRepository<>), typeof(CachedRepository<>));

    services.AddSingleton(typeof(IMemoryCache), typeof(MemoryCache));

    services.AddSingleton<ICurrentUser, CurrentUserService>();

    services.AddHostedService<Worker>();
});

var host = builder.Build();

host.Run();
