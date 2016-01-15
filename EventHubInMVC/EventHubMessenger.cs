using Microsoft.Extensions.Logging;
using Microsoft.ServiceBus.Messaging;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace EventHubInMVC
{
    internal sealed class EventHubMessenger : IMessenger, IDisposable
    {
        private readonly bool _closeOnDispose;
        private readonly EventHubClient _eventHubClient;
        private readonly ILogger<EventHubMessenger> _logger;

        public EventHubMessenger(EventHubClient eventHubClient, bool closeOnDispose, ILogger<EventHubMessenger> logger)
        {
            _eventHubClient = eventHubClient;
            _closeOnDispose = closeOnDispose;
            _logger = logger;
        }

        public async Task SendAsync(string message)
        {
            _logger.LogInformation("Sending a message to the EH");
            await _eventHubClient.SendAsync(new EventData(Encoding.UTF8.GetBytes(message)));
        }

        public void Dispose()
        {
            if (_closeOnDispose)
            {
                _eventHubClient.Close();
            }
        }
    }
}
