using Microsoft.EntityFrameworkCore;

namespace HouseKeeper.Core.States;
public class RenameDimensionState : DefaultState
{
    private readonly long _dimensionId;

    public RenameDimensionState(State state, long dimensionId) 
        : base(state)
    {
        _dimensionId = dimensionId;
    }

    protected override async Task<IState> InputTextInner(string text)
    {
        await using var context = await ApplicationContextFactory.Create();
        var dimension = await context.Dimensions
            .AsNoTracking()
            .Where(x => x.Id == _dimensionId)
            .FirstOrDefaultAsync();
        if (dimension != null)
        {
            dimension.Name = text;
            context.Update(dimension);
            await context.SaveChangesAsync();
            await Dialog.Send($"Dimension renamed to {text}");
            return new DefaultState(this);
        }
        else
        {
            await Dialog.Send($"Dataset does not exist");
            return new DefaultState(this);
        }
    }
}
