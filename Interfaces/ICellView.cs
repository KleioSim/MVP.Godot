using Godot;

namespace KleioSim.MVP.Godot.Interfaces;

public interface ICellView : IView
{
    Vector2I Location { get; }
}