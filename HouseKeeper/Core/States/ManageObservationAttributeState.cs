
using Microsoft.EntityFrameworkCore;

namespace HouseKeeper.Core.States;
public class ManageObservationAttributeState : DefaultState
{
    private readonly long _observationAttributeId;

    public ManageObservationAttributeState(State state, long observationAttributeId) 
        : base(state)
    {
        _observationAttributeId = observationAttributeId; 
    }

    public override async Task<IState> InputButton(int messageId, string buttonData)
    {
        await Dialog.ClearKeyboard(messageId);
        switch (buttonData)
        {
            case "rename":
                await Dialog.Send("Enter new name:");
                return new RenameObservationAttributeState(this, _observationAttributeId);
            case "delete":
                {
                    await using var context = await ApplicationContextFactory.Create();
                    var observationAttribute = await context.ObservationAttributes
                        .AsNoTracking()
                        .Where(x => x.Id == _observationAttributeId)
                        .FirstOrDefaultAsync();
                    if (observationAttribute != null)
                    {
                        context.Remove(observationAttribute);
                        await context.SaveChangesAsync();
                        await Dialog.Send("Observation attribute deleted");
                        return new DefaultState(this);
                    }
                    else
                    {
                        await Dialog.Send("Observation attribute does not exist");
                        return new DefaultState(this);
                    }
                }
            default:
                await Dialog.Send("Out of context");
                return new DefaultState(this);
        }
    }
}
