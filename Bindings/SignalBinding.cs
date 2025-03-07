using System;
using KleioSim.MVP.Godot.Interfaces;

namespace MVP.Godot.Bindings;

public class SignalBinding
{
    public readonly Func<object, object> controlGetter;
    public readonly object SignalName;
    public readonly Action<object, object> handlerAction;

    public SignalBinding(Func<object, object> contrlGetter, object signalName, Action<object, object> action)
    {
        controlGetter = contrlGetter;
        SignalName = signalName;
        handlerAction = action;
    }
}
