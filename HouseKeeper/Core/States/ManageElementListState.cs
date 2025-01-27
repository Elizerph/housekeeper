using Microsoft.EntityFrameworkCore;

namespace HouseKeeper.Core.States;
public class ManageElementListState : DefaultState
{
    private readonly long _dimensionId;

    public ManageElementListState(State state, long dimensionId) 
        : base(state)
    {
        _dimensionId = dimensionId;
    }

    public override async Task<IState> InputButton(int messageId, string buttonData)
    {
        await Dialog.ClearKeyboard(messageId);
        if (long.TryParse(buttonData, out var elementId))
        {
            await using var context = await ApplicationContextFactory.Create();
            var element = await context.Elements
                .AsNoTracking()
                .Where(x => x.Id == elementId)
                .FirstOrDefaultAsync();
            if (element != null)
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
                await Dialog.Send($"Manage {element.Name}:", buttons);
                return new ManageElementState(this, elementId);
            }
            else
            {
                await Dialog.Send("Element does not exist");
                return new DefaultState(this);
            }
        }
        else
        {
            switch (buttonData)
            {
                case "create":
                    await Dialog.Send("Enter new element name:");
                    return new CreateElementState(this, _dimensionId);
                default:
                    await Dialog.Send("Out of context");
                    return new DefaultState(this);
            }
        }
    }
}
