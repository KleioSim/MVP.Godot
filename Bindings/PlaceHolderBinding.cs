using Godot;
using System;

namespace MVP.Godot.Bindings;

public class PlaceHolderBinding
{
    public PlaceHolderBinding(Func<object, InstancePlaceholder> protypeGetter, Func<object, object, object> sourceGetter)
    {
        this.protypeGetter = protypeGetter;
        this.sourceGetter = sourceGetter;
    }

    public readonly Func<object, InstancePlaceholder> protypeGetter;
    public readonly Func<object, object, object> sourceGetter;
}
