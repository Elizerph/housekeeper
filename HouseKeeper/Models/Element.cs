using Newtonsoft.Json;

namespace HouseKeeper.Models;
public class Element : NamedEntity
{
    [JsonIgnore]
    public long DimensionId { get; set; }

    [JsonIgnore]
    public Dimension Dimension { get; set; }


    [JsonIgnore]
    public ICollection<Timeseries> TimeSeries { get; set; }
}
