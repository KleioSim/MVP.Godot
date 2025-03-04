namespace KleioSim.MVP.Godot.Interfaces;
public class Context
{
    private object data;

    public T GetData<T>()
    {
        return (T)data;
    }

    public void SetData(object data)
    {
        this.data= data;
    }
}