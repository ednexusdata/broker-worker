using OregonNexus.Broker.SharedKernel;
using OregonNexus.Broker.Domain;
using OregonNexus.Broker.Domain.Specifications;
using OregonNexus.Broker.Service.Resolvers;

namespace OregonNexus.Broker.Worker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly WorkerResolver _workerResolver;
    private readonly IRepository<Request> _requestsRepository;

    public Worker(ILogger<Worker> logger, WorkerResolver workerResolver, IRepository<Request> requestsRepository)
    {
        _logger = logger;
        _workerResolver = workerResolver;
        _requestsRepository = requestsRepository;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            
            var request = await _requestsRepository.FirstOrDefaultAsync(new RequestsReadyForProcessing());

            if (request is not null)
            {
                request.ProcessState = "Begin Processing";
                request.WorkerInstance = Environment.MachineName;

                await _requestsRepository.UpdateAsync(request);

                _logger.LogInformation("{requestId} is processing.", request.Id);

                await _workerResolver.ProcessAsync(request);
            }
            
            await Task.Delay(1000, stoppingToken);
        }
    }
}
