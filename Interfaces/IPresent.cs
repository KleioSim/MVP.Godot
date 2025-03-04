using System;
using System.Collections.Generic;

namespace KleioSim.MVP.Godot.Interfaces;

public interface IPresent
{
    IEnumerable<SignalBinding> SignalBindings { get; }
    IEnumerable<UpdateBinding> UpdateBinding { get; }
}

public class SignalBinding
{
    public readonly Func<object, object> controlGetter;
    public readonly object SignalName;
    public readonly Action<Context, object> handlerAction;

    public SignalBinding(Func<object, object> contrlGetter, object signalName, Action<Context, object> action)
    {
        this.controlGetter = contrlGetter;
        SignalName = signalName;
        this.handlerAction = action;
    }
}

public class UpdateBinding
{
    public readonly Action<object, object> targetSetter;
    public readonly Func<Context, object, object> sourceGetter;

    public UpdateBinding(Action<object, object> targetSetter, Func<Context, object, object> sourceGetter)
    {
        this.targetSetter = targetSetter;
        this.sourceGetter = sourceGetter;
    }
}