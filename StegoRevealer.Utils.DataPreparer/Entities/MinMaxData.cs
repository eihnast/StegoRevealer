namespace StegoRevealer.Utils.DataPreparer.Entities;

public class MinMaxData
{
    public int Min { get; set; }
    public int Max { get; set; }

    public override string ToString() => $"({Min}, {Max})";
}
