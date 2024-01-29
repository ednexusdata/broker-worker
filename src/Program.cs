using Microsoft.Extensions.Caching.Memory;
using OregonNexus.Broker.Data;
using OregonNexus.Broker.SharedKernel;
using OregonNexus.Broker.Worker;
using OregonNexus.Broker.Worker.Services;
using OregonNexus.Broker.Service;

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

    if (hostContext.Configuration["WorkerUserStamp"] is not null)
    {
        var current = new CurrentUserService(hostContext.Configuration["WorkerUserStamp"]!);
        services.AddSingleton<ICurrentUser>(current);
    }
    else
    {
        services.AddSingleton<ICurrentUser, CurrentUserService>();
    }
    
    
    if (hostContext.HostingEnvironment.IsDevelopment())
    {
        services.AddHttpClient("IgnoreSSL").ConfigurePrimaryHttpMessageHandler(() => {
            var httpClientHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) =>
                {
                    return true;
                }
            };
            return httpClientHandler;
        });
    }

    services.AddBrokerServicesForWorker();

    services.AddHostedService<Worker>();
});

var host = builder.Build();

host.Run();
