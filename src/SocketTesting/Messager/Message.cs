using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messager;

public abstract class Message(object sender)
{
    public object Sender { get; } = sender ?? throw new ArgumentNullException(nameof(sender));
}