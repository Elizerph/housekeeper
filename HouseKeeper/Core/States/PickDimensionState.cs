using Microsoft.EntityFrameworkCore;

namespace HouseKeeper.Core.States;
public class PickDimensionState : DefaultState
{
    private readonly long _datasetId;
    private readonly Dictionary<int, long> _selection;

    public PickDimensionState(State state, long datasetId, Dictionary<int, long> selection)
        : base(state)
    {
        _datasetId = datasetId;
        _selection = selection;
    }

    public override async Task<IState> InputButton(int messageId, string buttonData)
    {
        await Dialog.ClearKeyboard(messageId);
        if (long.TryParse(buttonData, out var elementId))
        {
            await using var context = await ApplicationContextFactory.Create();
            var dataset = await context.Datasets
                .AsNoTracking()
                .Include(x => x.Dimensions)
                    .ThenInclude(x => x.Elements)
                .Include(x => x.ObservationAttributes)
                .Where(x => x.Id == _datasetId)
                .FirstOrDefaultAsync();
            if (dataset != null)
            {
                var index = _selection.Count;
                _selection[index] = elementId;
                index++;
                if (index < dataset.Dimensions.Count)
                {
                    var dimension = dataset.Dimensions.ElementAt(index);
                    var buttons = dimension.Elements.Select(x => new MessageButton
                    { 
                        Label = x.Name,
                        Data = x.Id.ToString()
                    });
                    await Dialog.EditText(messageId, $"{dimension.Name} selected");
                    await Dialog.Send($"Pick element from {dimension.Name}:", buttons);
                    return new PickDimensionState(this, _datasetId, _selection);
                }
                else
                {
                    var firstObservationAttribute = dataset.ObservationAttributes.FirstOrDefault();
                    if (firstObservationAttribute != null)
                    {
                        await Dialog.Send($"Enter value for {firstObservationAttribute.Name}:");
                        return new PickObservationAttributeState(this, _datasetId, _selection, new Dictionary<int, string>());
                    }
                    else
                    {
                        await Dialog.Send($"Dataset {dataset.Name} has no observation attributes yet");
                        return new DefaultState(this);
                    }
                }
            }
            else
            {
                await Dialog.Send("Dataset does not exist");
                return new DefaultState(this);
            }
        }
        else
        {
            await Dialog.Send("Out of context");
            return new DefaultState(this);
        }
    }
}
