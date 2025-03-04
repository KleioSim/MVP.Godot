public class DemonModel : IDemonModel
{
    public uint Label { get; set; }
    public int Data => this.GetHashCode();
}
