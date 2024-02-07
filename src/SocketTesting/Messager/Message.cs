namespace Messager;

public abstract class Message(object sender)
{
    public object Sender { get; } = sender ?? throw new ArgumentNullException(nameof(sender));
}