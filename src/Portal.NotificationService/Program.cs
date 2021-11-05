using Portal.Domain.EmailContext.Services;
using Portal.NotificationService.Quartz;
using Portal.NotificationService.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Portal.NotificationService.Services;
using Quartz;
using Razor.Templating.Core;
using System.Threading.Tasks;

namespace Portal.NotificationService
{
	public class Program
	{
		private static IHostEnvironment _hostEnvironment = null;

		[System.Obsolete]
		public static async Task Main(string[] args)
		{
			var host = CreateHostBuilder(args).Build();

			await host.RunAsync();
		}

		[System.Obsolete]
		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.ConfigureLogging((hostContext, builder) =>
				{
					_hostEnvironment = hostContext.HostingEnvironment;
				})
				.UseWindowsService()
				.ConfigureServices((hostContext, services) =>
				{
					var configuration = hostContext.Configuration;
					RazorTemplateEngine.Initialize();
					var defaultConnectionString = configuration.GetConnectionString("DefaultConnection");
//					services.AddSingleton<ITimeService, TimeService>();
					services.AddScoped<EmailNotificationJob>();
					//services.AddSingleton<SmptClientFactory>();
					services.AddScoped<IEmailSender, EmailSender>();

					services.AddQuartz(q =>
					{
						q.UseMicrosoftDependencyInjectionScopedJobFactory();
						q.AddJobAndTrigger<EmailNotificationJob>(configuration);
					});

					services.AddQuartzHostedService(
							q => q.WaitForJobsToComplete = true);
				});

		private const string ApplicationName = "Portal Service";

	}
}
