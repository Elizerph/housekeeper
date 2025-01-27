using HouseKeeper.Models;

namespace HouseKeeper.Core.States;
public class CreateDimensionState : DefaultState
{
    private readonly long _datasetId;

    public CreateDimensionState(State state, long datasetId) 
        : base(state)
    {
        _datasetId = datasetId;
    }

    protected override async Task<IState> InputTextInner(string text)
    {
        await using var context = await ApplicationContextFactory.Create();
        await context.AddAsync(new Dimension
        {
            DatasetId = _datasetId,
            Name = text
        });
        await context.SaveChangesAsync();
        await Dialog.Send($"Dimension {text} created");
        return new DefaultState(this);
    }
}
