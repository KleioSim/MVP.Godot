using Godot;
using KleioSim.MVP.Godot.Interfaces;

namespace KleioSim.MVP.Godot;

public partial class Global : Node
{
    public override void _EnterTree()
    {
        MVPCore.Initialize();

        this.GetTree().Connect(SceneTree.SignalName.NodeAdded, Callable.From((Node node) =>
        {
            if(node is IView view) MVPCore.AddViewNode(view);
        }));

        this.GetTree().Connect(SceneTree.SignalName.NodeRemoved, Callable.From((Node node) =>
        {
            if (node is IView view) MVPCore.RemoveViewNode(view);
        }));

        this.GetTree().Connect(SceneTree.SignalName.ProcessFrame, Callable.From(() =>
        {
            MVPCore.UpdateViewNodes();
        }));
    }

    public override void _ExitTree()
    {
        MVPCore.Exit();
    }
}
