
using HouseKeeper.Models;
using HouseKeeper.Models.Stubs;

using Newtonsoft.Json;

namespace HouseKeeper.Core.States;
public class ImportDatasetState : DefaultState
{
    public ImportDatasetState(State state) 
        : base(state)
    {
    }

    public override async Task<IState> InputDocument(string fileId)
    {
        await using var stream = new MemoryStream();
        await Dialog.DowloadFile(fileId, stream);
        stream.Position = 0;
        using var reader = new StreamReader(stream);
        var content = await reader.ReadToEndAsync();
        var datasetStub = JsonConvert.DeserializeObject<DatasetStub>(content);

        await using var context = await ApplicationContextFactory.Create();
        var dataset = new Dataset
        {
            OwnerId = Dialog.UserId,
            Name = datasetStub.Name,
            Dimensions = datasetStub.Dimensions
                .Select(x => new Dimension
                {
                    Name = x.Name,
                    Elements = x.Elements.Select(y => new Element
                    {
                        Name = y
                    }).ToList()
                }).ToList(),
            ObservationAttributes = datasetStub.ObservationAttributes
                .Select(x => new ObservationAttribute
                {
                    Name = x
                }).ToList()
        };
        await context.AddAsync(dataset);
        await context.SaveChangesAsync();
        await Dialog.Send($"Dataset {dataset.Name} created");
        return new DefaultState(this);
    }
}
