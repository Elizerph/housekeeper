using HouseKeeper.Models.Stubs;

using Microsoft.EntityFrameworkCore;

using Newtonsoft.Json;

namespace HouseKeeper.Core.States;
public class ManageDatasetState : DefaultState
{
    private readonly long _datasetId;

    public ManageDatasetState(State state, long datasetId)
        : base(state)
    {
        _datasetId = datasetId;
    }

    public override async Task<IState> InputButton(int messageId, string buttonData)
    {
        await Dialog.ClearKeyboard(messageId);
        switch (buttonData)
        {
            case "export":
                {
                    await using var context = await ApplicationContextFactory.Create();
                    var dataset = await context.Datasets
                        .AsNoTracking()
                        .Include(x => x.Dimensions)
                            .ThenInclude(x => x.Elements)
                        .Include(x => x.ObservationAttributes)
                        .FirstOrDefaultAsync();
                    if (dataset != null)
                    {
                        var datasetStub = new DatasetStub
                        {
                            Name = dataset.Name,
                            Dimensions = dataset.Dimensions
                                .Select(x => new DimensionStub
                                {
                                    Name = x.Name,
                                    Elements = x.Elements.Select(y => y.Name)
                                        .ToArray()
                                })
                                .ToArray(),
                            ObservationAttributes = dataset.ObservationAttributes
                                .Select(x => x.Name)
                                .ToArray()
                        };
                        var content = JsonConvert.SerializeObject(datasetStub, Formatting.Indented);
                        await Dialog.SendTextFile(dataset.Name, $"{dataset.Name}.json", new[] { content }.ToAsyncEnumerable());
                        return new DefaultState(this);
                    }
                    else
                    {
                        await Dialog.Send("Dataset does not exist");
                        return new DefaultState(this);
                    }
                }
            case "delete":
                {
                    await using var context = await ApplicationContextFactory.Create();
                    var dataset = await context.Datasets
                        .AsNoTracking()
                        .Where(x => x.Id == _datasetId)
                        .FirstOrDefaultAsync();
                    if (dataset != null)
                    {
                        context.Remove(dataset);
                        await context.SaveChangesAsync();
                        await Dialog.Send("Dataset deleted");
                        return new DefaultState(this);
                    }
                    else
                    {
                        await Dialog.Send("Dataset does not exist");
                        return new DefaultState(this);
                    }
                }
            default:
                await Dialog.Send("Out of context");
                return new DefaultState(this);
        }
    }
}
