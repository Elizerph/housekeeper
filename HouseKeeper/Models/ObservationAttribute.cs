using Newtonsoft.Json;

namespace HouseKeeper.Models;
public class ObservationAttribute : NamedEntity
{
    [JsonIgnore]
    public long DatasetId { get; set; }

    [JsonIgnore]
    public Dataset Dataset { get; set; }

    [JsonIgnore]
    public ICollection<ObservationAttributeValue> ObservationAttributeValues { get; set; }
}
