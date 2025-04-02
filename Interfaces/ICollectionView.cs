using Godot;

namespace KleioSim.MVP.Godot.Interfaces;

public interface ICollectionView : IView
{
    InstancePlaceholder PlaceHolder {  get; } 
}