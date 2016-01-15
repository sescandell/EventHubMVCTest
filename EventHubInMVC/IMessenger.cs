using System.Threading.Tasks;

namespace EventHubInMVC
{
    public interface IMessenger
    {
        Task SendAsync(string message);
    }
}