using Newtonsoft.Json;

namespace HouseKeeper.Models;
public abstract class BaseEntity
{
    [JsonIgnore]
    public long Id { get; set; }
}
