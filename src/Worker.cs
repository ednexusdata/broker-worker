using OregonNexus.Broker.SharedKernel;
using OregonNexus.Broker.Domain;
using OregonNexus.Broker.Domain.Specifications;

namespace OregonNexus.Broker.Worker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IRepository<Request> _requestsRepository;

    public Worker(ILogger<Worker> logger, IRepository<Request> requestsRepository)
    {
        _logger = logger;
        _requestsRepository = requestsRepository;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            
            var requests = await _requestsRepository.FirstOrDefaultAsync(new RequestsReadyForProcessing());

            if (requests is not null)
            {
                requests.ProcessState = "Begin Processing";
                requests.WorkerInstance = Environment.MachineName;

                await _requestsRepository.UpdateAsync(requests);

                _logger.LogInformation("{requestId} request waiting to process.", requests?.Id);
            }
            
            await Task.Delay(1000, stoppingToken);
        }
    }
}
