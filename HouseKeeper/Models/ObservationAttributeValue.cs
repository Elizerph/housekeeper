namespace HouseKeeper.Models;
public class ObservationAttributeValue : BaseEntity
{
    public string Value { get; set; }
    public long ObservationId { get; set; }
    public Observation Observation { get; set; }
    public long AttributeId { get; set; }
    public ObservationAttribute Attribute { get; set; }
}
