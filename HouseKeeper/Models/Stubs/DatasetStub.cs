namespace HouseKeeper.Models.Stubs;
public class DatasetStub
{
    public static DatasetStub Temlpate { get; } = new DatasetStub
    {
        Name = "Template dataset",
        Dimensions =
            [
                new() {
                    Name = "Dimension A",
                    Elements =
                    [
                        "Element a1",
                        "Element a2",
                        "Element a3"
                    ]
                },
                new() {
                    Name = "Dimension B",
                    Elements =
                    [
                        "Element b1",
                        "Element b2",
                        "Element b3"
                    ]
                },
                new() {
                    Name = "Dimension C",
                    Elements =
                    [
                        "Element c1",
                        "Element c2",
                        "Element c3"
                    ]
                }
            ],
        ObservationAttributes =
        [
            "Attribute X",
            "Attribute Y",
            "Attribute Z",
        ]
    };
    public string Name { get; set; }
    public IReadOnlyCollection<DimensionStub> Dimensions { get; set; }
    public IReadOnlyCollection<string> ObservationAttributes { get; set; }
}
