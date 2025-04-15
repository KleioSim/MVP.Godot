using System.Collections.Generic;
using MVP.Godot.Bindings;

namespace KleioSim.MVP.Godot.Interfaces;

public interface IPresent
{
    IEnumerable<SignalBinding> SignalBindings { get; }
    IEnumerable<UpdateBinding> UpdateBinding { get; }
    IEnumerable<CollectionBinding> CollectionBinding { get; }
    IEnumerable<TilemapBinding> TilemapBindings { get; }

    IEnumerable<PlaceHolderBinding> PlaceHolderBindings { get; }
    IEnumerable<PackedSceneBinding> PackedSceneBindings { get; }
}
