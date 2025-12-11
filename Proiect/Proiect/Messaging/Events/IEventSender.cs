using System.Threading.Tasks;

namespace Proiect.Messaging.Events
{
    public interface IEventSender
    {
        Task SendAsync<T>(string topicName, T @event);
    }
}