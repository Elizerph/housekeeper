using Microsoft.EntityFrameworkCore;

namespace HouseKeeper.Core.States;
public class RenameObservationAttributeState : DefaultState
{
    private readonly long _observationAttributeId;

    public RenameObservationAttributeState(State state, long observationAttributeId) 
        : base(state)
    {
        _observationAttributeId = observationAttributeId;
    }

    protected override async Task<IState> InputTextInner(string text)
    {
        await using var context = await ApplicationContextFactory.Create();
        var observationAttribute = await context.ObservationAttributes
            .AsNoTracking()
            .Where(x => x.Id == _observationAttributeId)
            .FirstOrDefaultAsync();
        if (observationAttribute != null)
        {
            observationAttribute.Name = text;
            context.Update(observationAttribute);
            await context.SaveChangesAsync();
            await Dialog.Send($"Observation attribute renamed to {text}");
            return new DefaultState(this);
        }
        else
        {
            await Dialog.Send($"Observation attribute does not exist");
            return new DefaultState(this);
        }
    }
}
