using Godot;
using KleioSim.MVP.Godot.Interfaces;

public partial class DemonView : Control, IView
{
    public Label Label => GetNode<Label>("Label");
    public Button CreateButton => GetNode<Button>("Create");
}
