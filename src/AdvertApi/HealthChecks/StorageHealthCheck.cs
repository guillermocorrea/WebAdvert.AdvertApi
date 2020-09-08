using System.Threading;
using System.Threading.Tasks;
using AdvertApi.Services;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace AdvertApi.HealthChecks
{
  public class StorageHealthCheck : IHealthCheck
  {
    private readonly IAdvertStorageService _storageService;
    private readonly ILogger<StorageHealthCheck> _logger;
    public StorageHealthCheck(IAdvertStorageService storageService, ILogger<StorageHealthCheck> logger)
    {
      _logger = logger;
      _logger.LogInformation("StorageHealthCheck ctor");
      _storageService = storageService;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
      var isStorageOk = await _storageService.HealthCheckAsync();
      _logger.LogInformation($"StorageHealthCheck CheckAsync {isStorageOk}");

      if (isStorageOk)
      {
        return HealthCheckResult.Healthy("A healthy result.");
      }

      return HealthCheckResult.Unhealthy("Unhealthy.");
    }
  }
}