using Microsoft.EntityFrameworkCore;

namespace HouseKeeper.Core.States;
public class RenameDatasetState : DefaultState
{
    private readonly long _datasetId;

    public RenameDatasetState(State state, long datasetId) 
        : base(state)
    {
        _datasetId = datasetId;
    }

    protected override async Task<IState> InputTextInner(string text)
    {
        await using var context = await ApplicationContextFactory.Create();
        var dataset = await context.Datasets
            .AsNoTracking()
            .Where(x => x.Id == _datasetId)
            .FirstOrDefaultAsync();
        if (dataset != null)
        {
            dataset.Name = text;
            context.Update(dataset);
            await context.SaveChangesAsync();
            await Dialog.Send($"Dataset renamed to {text}");
            return new DefaultState(this);
        }
        else
        {
            await Dialog.Send($"Dataset does not exist");
            return new DefaultState(this);
        }
    }
}
