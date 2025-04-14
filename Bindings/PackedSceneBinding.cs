using Godot;
using System;

namespace MVP.Godot.Bindings;

public class PackedSceneBinding
{
    public PackedSceneBinding(Func<object, PackedScene> protypeGetter, Func<object, object, object> sourceGetter)
    {
        this.protypeGetter = protypeGetter;
        this.sourceGetter = sourceGetter;
    }

    public readonly Func<object, PackedScene> protypeGetter;
    public readonly Func<object, object, object> sourceGetter;
}