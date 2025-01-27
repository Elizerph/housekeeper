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
            case "rename":
                await Dialog.Send("Enter new name:");
                return new RenameDatasetState(this, _datasetId);
            case "dimensions":
                {
                    await using var context = await ApplicationContextFactory.Create();
                    var dataset = await context.Datasets
                        .AsNoTracking()
                        .Include(x => x.Dimensions)
                        .Where(x => x.Id == _datasetId)
                        .FirstOrDefaultAsync();
                    if (dataset != null)
                    {
                        var buttons = dataset.Dimensions.Select(x => new MessageButton
                        {
                            Label = x.Name,
                            Data = x.Id.ToString()
                        }).ToList();
                        buttons.Add(new MessageButton
                        {
                            Label = "Create",
                            Data = "create"
                        });
                        await Dialog.Send($"Manage dimensions of {dataset.Name}:", buttons);
                        return new ManageDimensionListState(this, _datasetId);
                    }
                    else
                    {
                        await Dialog.Send("Dataset does not exist");
                        return new DefaultState(this);
                    }
                }
            case "observation_attributes":
                {
                    await using var context = await ApplicationContextFactory.Create();
                    var dataset = await context.Datasets
                        .AsNoTracking()
                        .Include(x => x.ObservationAttributes)
                        .Where(x => x.Id == _datasetId)
                        .FirstOrDefaultAsync();
                    if (dataset != null)
                    {
                        var buttons = dataset.ObservationAttributes.Select(x => new MessageButton
                        {
                            Label = x.Name,
                            Data = x.Id.ToString()
                        }).ToList();
                        buttons.Add(new MessageButton
                        {
                            Label = "Create",
                            Data = "create"
                        });
                        await Dialog.Send($"Manage observation attributes of {dataset.Name}:", buttons);
                        return new ManageObservationAttributeListState(this, _datasetId);
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
