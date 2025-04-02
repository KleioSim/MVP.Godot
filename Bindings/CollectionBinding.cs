using Godot;
using KleioSim.MVP.Godot.Interfaces;
using System;
using System.Collections.Generic;

namespace MVP.Godot.Bindings;

public class CollectionBinding
{
    public CollectionBinding(Func<object, InstancePlaceholder> protypeGetter, Func<object, object, IEnumerable<object>> sourceGetter)
    {
        this.protypeGetter = protypeGetter;
        this.sourceGetter = sourceGetter;
    }

    public readonly Func<object, InstancePlaceholder> protypeGetter;
    public readonly Func<object, object, IEnumerable<object>> sourceGetter;
}

public class CollectionBinding2
{
    public readonly Func<object, ICollectionView> protypeGetter;
    public readonly Func<object, object, IEnumerable<object>> sourceGetter;

    public CollectionBinding2(Func<object, ICollectionView> protypeGetter, Func<object, object, IEnumerable<object>> sourceGetter)
    {
        this.protypeGetter = protypeGetter;
        this.sourceGetter = sourceGetter;
    }
}