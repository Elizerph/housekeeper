using HouseKeeper.Models;

namespace HouseKeeper.Core.States;
public class CreateObservationsAttributeState : DefaultState
{
    private readonly long _datasetId;

    public CreateObservationsAttributeState(State state, long datasetId) 
        : base(state)
    {
        _datasetId = datasetId;
    }

    protected override async Task<IState> InputTextInner(string text)
    {
        await using var context = await ApplicationContextFactory.Create();
        await context.AddAsync(new ObservationAttribute
        {
            DatasetId = _datasetId,
            Name = text
        });
        await context.SaveChangesAsync();
        await Dialog.Send($"Element {text} created");
        return new DefaultState(this);
    }
}
