using System;
using System.Reflection;

namespace KleioSim.MVP.Godot;

public class Decorator : DispatchProxy
{
    public static Action OnDataChanged { get; set; }

    private object _decorated;

    protected override object Invoke(MethodInfo targetMethod, object[] args)
    {
        var result = targetMethod.Invoke(_decorated, args);
        if(targetMethod.Name == "OnMessage" || targetMethod.Name == "NextTurn")
        {
            OnDataChanged?.Invoke();
        }

        return result;
    }

    public static T Create<T>(T decorated)
    {
        object proxy = Create<T, Decorator>();
        ((Decorator)proxy).SetParameters(decorated);

        OnDataChanged?.Invoke();

        return (T)proxy;
    }

    private void SetParameters<T>(T decorated)
    {
        if (decorated == null)
        {
            throw new ArgumentNullException(nameof(decorated));
        }
        _decorated = decorated;
    }
}
