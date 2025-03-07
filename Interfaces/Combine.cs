namespace KleioSim.MVP.Godot.Interfaces;
class Combine
{
    public IPresent present;
    public IView view;

    public bool IsDirty { get; internal set; } = true;

    public Combine(IView view, IPresent present)
    {
        this.view = view;
        this.present = present;
    }
}
