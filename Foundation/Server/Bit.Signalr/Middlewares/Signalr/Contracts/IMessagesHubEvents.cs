﻿using System.Threading.Tasks;

namespace Bit.Signalr.Middlewares.Signalr.Contracts
{
    public interface IMessagesHubEvents
    {
        Task OnConnected(MessagesHub hub);

        Task OnDisconnected(MessagesHub hub, bool stopCalled);

        Task OnReconnected(MessagesHub hub);
    }
}
