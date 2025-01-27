using Microsoft.EntityFrameworkCore;

namespace HouseKeeper.Core.States;
public class RenameElementState : DefaultState
{
    private readonly long _elementId;

    public RenameElementState(State state, long elementId) 
        : base(state)
    {
        _elementId = elementId;
    }

    protected override async Task<IState> InputTextInner(string text)
    {
        await using var context = await ApplicationContextFactory.Create();
        var element = await context.Elements
            .AsNoTracking()
            .Where(x => x.Id == _elementId)
            .FirstOrDefaultAsync();
        if (element != null)
        {
            element.Name = text;
            context.Update(element);
            await context.SaveChangesAsync();
            await Dialog.Send($"Element renamed to {text}");
            return new DefaultState(this);
        }
        else
        {
            await Dialog.Send($"Element does not exist");
            return new DefaultState(this);
        }
    }
}
