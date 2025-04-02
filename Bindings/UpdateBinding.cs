using System;

namespace MVP.Godot.Bindings;

public class UpdateBinding
{
    public readonly Action<object, object> targetSetter;
    public readonly Func<object, object, object> sourceGetter;

    public UpdateBinding(Action<object, object> targetSetter, Func<object, object, object> sourceGetter)
    {
        this.targetSetter = targetSetter;
        this.sourceGetter = sourceGetter;
    }
}
