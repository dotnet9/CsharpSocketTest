using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messager
{
    public interface IMessenger
    {
        void Subscribe<TMessage>(object recipient, Action<TMessage> action,
            ThreadOption threadOption = ThreadOption.PublisherThread, string? tag = null) where TMessage : Message;

        void Unsubscribe<TMessage>(object recipient, Action<TMessage>? action = null) where TMessage : Message;

        void Publish<TMessage>(object sender, TMessage message, string? tag = null) where TMessage : Message;
    }
}