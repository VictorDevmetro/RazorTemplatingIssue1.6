using Portal.Domain.EmailContext.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using System.Threading.Tasks;

namespace Portal.NotificationService.Quartz
{
	[DisallowConcurrentExecution]
	public class EmailNotificationJob
		: JobBase
	{
		protected override string JobName => "Email Notification";

		public EmailNotificationJob(IServiceScopeFactory serviceScopeFactory, ILogger<EmailNotificationJob> logger)
			: base(serviceScopeFactory, logger)
		{
		}

		protected override async Task ExecuteJob(IJobExecutionContext context, IServiceScope serviceScope) =>
			await serviceScope.ServiceProvider.GetService<IEmailSender>()
				.SendQueuedEmails(context.CancellationToken);
	}
}
