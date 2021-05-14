using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VM_IP_VOD_CP_File_Manager.Application;
using VM_IP_VOD_CP_File_Manager.BusinessLogic.Contracts;

namespace VM_IP_VOD_CP_File_Manager.Service
{
    public class Worker : BackgroundService
    {
        private int _pollTime;
        private readonly AppConfig _options;
        private readonly ILogger<Worker> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public Worker(IServiceScopeFactory scopeFactory,
            ILogger<Worker> logger,
            IOptions<AppConfig> options)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _options = options.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Workflow Services Started Successfully, Operations starting.");
                var tokenSource = new CancellationTokenSource();
                var token = tokenSource.Token;
                if (_options != null)
                    _pollTime = Convert.ToInt32(_options.PollIntervalInSeconds) * 1000;
                else
                {
                    tokenSource.Cancel();
                }

                while (!token.IsCancellationRequested)
                {
                    _logger.LogInformation("########## Starting Processing Workflow ##########");
                    var scope = _scopeFactory.CreateScope();
                    var cpFileProcessor = scope.ServiceProvider.GetRequiredService<IWorkflowProcessor>();
                    await cpFileProcessor.StartAsync(cancellationToken);
                    _logger.LogInformation("########## Completed Processing Workflow ##########");
                    await Task.Delay(_pollTime, cancellationToken);
                }
            }
            catch (Exception eAException)
            {
                _logger.LogError($"Exception: {eAException.Message}");
            }
            finally
            {
                _logger.LogError("Service stopping.");
            }

        }
    }
}
