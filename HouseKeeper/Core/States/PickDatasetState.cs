using Microsoft.EntityFrameworkCore;

namespace HouseKeeper.Core.States;
public class PickDatasetState : DefaultState
{
    public PickDatasetState(State state)
        : base(state)
    {
    }

    public override async Task<IState> InputButton(int messageId, string buttonData)
    {
        await Dialog.ClearKeyboard(messageId);
        if (long.TryParse(buttonData, out var datasetId))
        {
            await using var context = await ApplicationContextFactory.Create();
            var dataset = await context.Datasets
                .AsNoTracking()
                .Include(x => x.Dimensions)
                    .ThenInclude(x => x.Elements)
                .Where(x => x.Id == datasetId)
                .FirstOrDefaultAsync();
            if (dataset != null)
            {
                await Dialog.EditText(messageId, $"{dataset.Name} selected");
                var firstDimension = dataset.Dimensions.FirstOrDefault();
                if (firstDimension != null)
                {
                    var buttons = firstDimension.Elements.Select(x => new MessageButton
                    {
                        Label = x.Name,
                        Data = x.Id.ToString()
                    }).ToList();
                    await Dialog.Send($"Pick element from {firstDimension.Name}:", buttons);
                    return new PickDimensionState(this, datasetId, new Dictionary<int, long>());
                }
                else
                {
                    await Dialog.Send($"Dataset {dataset.Name} has no dimensions yet");
                    return new DefaultState(this);
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
