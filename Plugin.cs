#if TOOLS
using Godot;
using System;

namespace KleioSim.MVP.Godot;

[Tool]
public partial class Plugin : EditorPlugin
{
	public override void _EnterTree()
	{
        AddAutoloadSingleton("MVP_Global", "res://addons/KleioSim.MVP.Godot/Global.cs");
    }

	public override void _ExitTree()
	{
        RemoveAutoloadSingleton("MVP_Global");
    }
}
#endif
