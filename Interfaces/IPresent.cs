using System.Collections.Generic;
using MVP.Godot.Bindings;

namespace KleioSim.MVP.Godot.Interfaces;

public interface IPresent
{
    IEnumerable<SignalBinding> SignalBindings { get; }
    IEnumerable<UpdateBinding> UpdateBinding { get; }
    IEnumerable<CollectionBinding> CollectionBinding { get; }
    IEnumerable<CollectionBinding2> CollectionBinding2 { get; }
    IEnumerable<TilemapBinding> TilemapBindings { get; }
}
