using System.Threading;
using System.Threading.Tasks;

namespace Proiect.Messaging.Events
{
    public interface IEventListener
    { 

        Task StartAsync(string topicName, string subscriptionName, CancellationToken cancellationToken);

        Task StopAsync(CancellationToken cancellationToken);
    }
}