using Microsoft.EntityFrameworkCore;

namespace HouseKeeper.Core.States;
public class ManageObservationAttributeListState : DefaultState
{
    private readonly long _datasetId;

    public ManageObservationAttributeListState(State state, long datasetId) 
        : base(state)
    {
        _datasetId = datasetId;
    }

    public override async Task<IState> InputButton(int messageId, string buttonData)
    {
        await Dialog.ClearKeyboard(messageId);
        if (long.TryParse(buttonData, out var observationAttributeId))
        {
            await using var context = await ApplicationContextFactory.Create();
            var observationAttribute = await context.ObservationAttributes
                .AsNoTracking()
                .Where(x => x.Id == observationAttributeId)
                .FirstOrDefaultAsync();
            if (observationAttribute != null)
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
                        Label = "Delete",
                        Data = "delete"
                    }
                };
                await Dialog.Send($"Manage {observationAttribute.Name}:", buttons);
                return new ManageObservationAttributeState(this, observationAttributeId);
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
                    await Dialog.Send("Enter new observation attribute name:");
                    return new CreateObservationsAttributeState(this, _datasetId);
                default:
                    await Dialog.Send("Out of context");
                    return new DefaultState(this);
            }
        }
    }
}
