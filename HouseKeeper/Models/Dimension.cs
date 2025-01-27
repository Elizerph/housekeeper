using Newtonsoft.Json;

namespace HouseKeeper.Models;
public class Dimension : NamedEntity
{
    [JsonIgnore]
    public long DatasetId { get; set; }

    [JsonIgnore]
    public Dataset Dataset { get; set; }

    public ICollection<Element> Elements { get; set; }
}
