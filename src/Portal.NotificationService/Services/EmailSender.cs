using Microsoft.Extensions.Logging;
using Portal.Domain.EmailContext.Services;
using Portal.EmailTemplates.ViewModels;
using Razor.Templating.Core;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Portal.NotificationService.Services
{
	public class EmailSender
		: IEmailSender
	{
		protected ILogger Logger { get; private set; }


		public EmailSender(ILogger<EmailSender> logger)
		{
			Logger = logger;
		}

			public async Task SendQueuedEmails(CancellationToken cancellationToken)
		{
			try
			{
				var body = await GenerateContent();
			}
			catch (System.Exception ex)
			{
				Logger.LogError(ex.Message);
			}
		}

		private static async Task<string> GenerateContent()
		{
			var viewData = CreateViewData();
			var viewModel = CreateViewModel();

			return await RazorTemplateEngine.RenderAsync($"~/Views/Template.cshtml", viewModel, viewData);
		}

		private static CustomViewModel CreateViewModel()
		{
			return new CustomViewModel
			{
				MyProperty = 123654
			};
		}

		private static Dictionary<string, object> CreateViewData()
		{
			var viewData = new Dictionary<string, object>
						{
								{ "WebSiteUrl", "localhost"},
						};

			return viewData;
		}
	}
}
