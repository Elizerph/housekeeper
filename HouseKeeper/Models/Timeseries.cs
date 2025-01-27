namespace HouseKeeper.Models;
public class Timeseries : BaseEntity
{
    public long DatasetId { get; set; }
    public Dataset Dataset { get; set; }
    public string Mnemonics { get; set; }
    public ICollection<Element> Elements { get; set; }
    public ICollection<Observation> Observations { get; set; }
}
