﻿using Godot;
using KleioSim.MVP.Godot;

public class DemonPresenter : Present<DemonView, IDemonModel>
{
    static DemonPresenter()
    {
        BindProperty(v => v.Label.Text, (m) => m.Data.ToString());
        BindSignal(v => v.CreateButton, Button.SignalName.Pressed, (_) => MVPCore.CreateSession<IDemonModel>(new DemonModel()));
    }
}

public interface IDemonModel
{
    int Data { get; }
}