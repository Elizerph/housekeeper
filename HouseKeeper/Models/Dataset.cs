using Newtonsoft.Json;

namespace HouseKeeper.Models;
public class Dataset : NamedEntity
{
    [JsonIgnore]
    public long OwnerId { get; set; }

    [JsonIgnore]
    public User Owner { get; set; }

    public ICollection<Dimension> Dimensions { get; set; }

    public ICollection<ObservationAttribute> ObservationAttributes { get; set; }

    [JsonIgnore]
    public ICollection<Timeseries> Timeseries { get; set; }
}
