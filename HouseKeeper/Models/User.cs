namespace HouseKeeper.Models;
public class User : BaseEntity
{
    public List<Dataset> Datasets { get; set; }
}
