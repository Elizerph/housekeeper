using HouseKeeper.Models;

namespace HouseKeeper.Core.States;
public class CreateElementState : DefaultState
{
    private readonly long _dimensionId;

    public CreateElementState(State state, long dimensionId) 
        : base(state)
    {
        _dimensionId = dimensionId;
    }

    protected override async Task<IState> InputTextInner(string text)
    {
        await using var context = await ApplicationContextFactory.Create();
        await context.AddAsync(new Element
        {
            DimensionId = _dimensionId,
            Name = text
        });
        await context.SaveChangesAsync();
        await Dialog.Send($"Element {text} created");
        return new DefaultState(this);
    }
}
