namespace HouseKeeper.Models;
public class Observation : BaseEntity
{
    public long TimeseriesId { get; set; }
    public Timeseries Timeseries { get; set; }
    public DateTime Timestamp { get; set; }
    public ICollection<ObservationAttributeValue> Values { get; set; }
}
