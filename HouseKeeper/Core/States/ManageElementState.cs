using Microsoft.EntityFrameworkCore;

namespace HouseKeeper.Core.States;
public class ManageElementState : DefaultState
{
    private readonly long _elementId;

    public ManageElementState(State state, long elementId) 
        : base(state)
    {
        _elementId = elementId; 
    }

    public override async Task<IState> InputButton(int messageId, string buttonData)
    {
        await Dialog.ClearKeyboard(messageId);
        switch (buttonData)
        {
            case "rename":
                await Dialog.Send("Enter new name:");
                return new RenameElementState(this, _elementId);
            case "delete":
                {
                    await using var context = await ApplicationContextFactory.Create();
                    var element = await context.Elements
                        .AsNoTracking()
                        .Where(x => x.Id == _elementId)
                        .FirstOrDefaultAsync();
                    if (element != null)
                    {
                        context.Remove(element);
                        await context.SaveChangesAsync();
                        await Dialog.Send("Element deleted");
                        return new DefaultState(this);
                    }
                    else
                    {
                        await Dialog.Send("Element does not exist");
                        return new DefaultState(this);
                    }
                }
            default:
                await Dialog.Send("Out of context");
                return new DefaultState(this);
        }
    }
}
