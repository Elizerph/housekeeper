using Microsoft.EntityFrameworkCore;

namespace HouseKeeper.Core.States;
public class ManageDimensionListState : DefaultState
{
    private readonly long _datasetId;

    public ManageDimensionListState(State state, long datasetId) 
        : base(state)
    {
        _datasetId = datasetId;
    }

    public override async Task<IState> InputButton(int messageId, string buttonData)
    {
        await Dialog.ClearKeyboard(messageId);
        if (long.TryParse(buttonData, out var dimensionId))
        {
            await using var context = await ApplicationContextFactory.Create();
            var dimension = await context.Dimensions
                .AsNoTracking()
                .Where(x => x.Id == dimensionId)
                .FirstOrDefaultAsync();
            if (dimension != null)
            {
                var buttons = new[]
                {
                    new MessageButton
                    {
                        Label = "Rename",
                        Data = "rename"
                    },
                    new MessageButton
                    {
                        Label = "Elements",
                        Data = "elements"
                    },
                    new MessageButton
                    {
                        Label = "Delete",
                        Data = "delete"
                    }
                };
                await Dialog.Send($"Manage {dimension.Name}:", buttons);
                return new ManageDimensionState(this, dimensionId);
            }
            else
            {
                await Dialog.Send("Dimension does not exist");
                return new DefaultState(this);
            }
        }
        else
        {
            switch (buttonData)
            {
                case "create":
                    await Dialog.Send("Enter new dimension name:");
                    return new CreateDimensionState(this, _datasetId);
                default:
                    await Dialog.Send("Out of context");
                    return new DefaultState(this);
            }
        }
    }
}
