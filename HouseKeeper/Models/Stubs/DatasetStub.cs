namespace HouseKeeper.Models.Stubs;
public class DatasetStub
{
    public string Name { get; set; }
    public IReadOnlyCollection<DimensionStub> Dimensions { get; set; }
    public IReadOnlyCollection<string> ObservationAttributes { get; set; }
}
