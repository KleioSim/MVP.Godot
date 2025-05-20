using System;

namespace MVP.Godot.Bindings;

public class UpdateBinding
{
    public readonly Action<object, object> targetSetter;
    public readonly Func<object, object, object> sourceGetter;

    public readonly bool freshOnlyVisable;

    public UpdateBinding(Action<object, object> targetSetter, Func<object, object, object> sourceGetter, bool freshOnlyVisable)
    {
        this.targetSetter = targetSetter;
        this.sourceGetter = sourceGetter;
        this.freshOnlyVisable = freshOnlyVisable;
    }
}
