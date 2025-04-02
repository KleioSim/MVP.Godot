using System;
using System.Collections.Generic;
using Godot;
using KleioSim.MVP.Godot.Interfaces;

namespace MVP.Godot.Bindings;

public class TilemapBinding
{
    public readonly Func<IView, TileMapLayer> tilemapGetter;
    public readonly Func<object, object, IReadOnlyDictionary<Vector2I, object>> sourceGetter;

    public TilemapBinding(Func<IView, TileMapLayer> tilemapGetter, Func<object, object, IReadOnlyDictionary<Vector2I, object>> sourceGetter)
    {
        this.tilemapGetter = tilemapGetter;
        this.sourceGetter = sourceGetter;
    }
}