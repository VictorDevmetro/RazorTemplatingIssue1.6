#if DEBUG
//#define DEBUG_LIFETIME
#endif

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Threading.Tasks;

namespace Portal.NotificationService.Quartz
{
	[DisallowConcurrentExecution]
	public abstract class JobBase
		: IJob
	{
#if DEBUG_LIFETIME
		private static ImmutableList<string> _runningJobs = ImmutableList<string>.Empty;

		private static SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
#endif

		private readonly IServiceScopeFactory _serviceScopeFactory;

		private const int LogIsRequiredThresholdInMinutes = 59;

		protected abstract Task ExecuteJob(IJobExecutionContext context, IServiceScope serviceScope);

		protected abstract string JobName { get; }

		protected ILogger Logger { get; private set; }

		protected JobBase(IServiceScopeFactory serviceScopeFactory, ILogger loogger)
		{
			_serviceScopeFactory = serviceScopeFactory;
			Logger = loogger;
		}

		public async Task Execute(IJobExecutionContext context)
		{
			try
			{
#if DEBUG_LIFETIME
				await _lock.WaitAsync();
				try
				{
					_runningJobs = _runningJobs.Add(JobName);
				}
				finally
				{
					_lock.Release();
				}

				var runningJobs = _runningJobs;
				Logger.LogInformation($"Job started. Running jobs ({runningJobs.Count}): " +
															$"{Environment.NewLine}\t'{string.Join($"',{Environment.NewLine}\t'", runningJobs)}'");
#else
				var needLogRuntime = RuntimeLogIsRequired(context);

				if (needLogRuntime)
					Logger.LogInformation("started.");
#endif

				using (var serviceScope = _serviceScopeFactory.CreateScope())
					await ExecuteJob(context, serviceScope);

#if DEBUG_LIFETIME
				await _lock.WaitAsync();
				try
				{
					_runningJobs = _runningJobs.Remove(JobName);
				}
				finally
				{
					_lock.Release();
				}

				runningJobs = _runningJobs;
				Logger.LogInformation($"Job stopped. Running jobs ({runningJobs.Count}): " +
					$"{Environment.NewLine}\t'{string.Join($"',{Environment.NewLine}\t'", runningJobs)}'");
#else
				if (needLogRuntime)
					Logger.LogInformation("stopped");
#endif
			}
			catch (Exception ex)
			{
#if DEBUG_LIFETIME
				await _lock.WaitAsync();
				try
				{
					_runningJobs = _runningJobs.Remove(JobName);
				}
				finally
				{
					_lock.Release();
				}

				Logger.LogError(ex, $"Job failed. Running jobs {_runningJobs.Count}", ex);
#else
				Logger.LogError(ex, "failed");
#endif
			}
		}

		private static bool RuntimeLogIsRequired(IJobExecutionContext context)
		{
			var nextRuntimeUtc = context.Trigger.GetNextFireTimeUtc();
			if (!nextRuntimeUtc.HasValue)
				return true;

			return (nextRuntimeUtc.Value.UtcDateTime - DateTime.UtcNow).TotalMinutes > LogIsRequiredThresholdInMinutes;
		}
	}
}