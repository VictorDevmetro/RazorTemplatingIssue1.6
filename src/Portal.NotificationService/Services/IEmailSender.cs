using System.Threading;
using System.Threading.Tasks;

namespace Portal.Domain.EmailContext.Services
{
	public interface IEmailSender
	{
		Task SendQueuedEmails(CancellationToken cancellationToken);
	}
}
