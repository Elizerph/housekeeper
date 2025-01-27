using HouseKeeper.Models;

namespace HouseKeeper.Core.States;
public class CreateDatasetState : DefaultState
{
    public CreateDatasetState(State state) 
        : base(state)
    {
    }

    protected override async Task<IState> InputTextInner(string text)
    {
        await using var context = await ApplicationContextFactory.Create();
        await context.AddAsync(new Dataset
        {
            OwnerId = Dialog.UserId,
            Name = text
        });
        await context.SaveChangesAsync();
        await Dialog.Send($"Dataset {text} created");
        return new DefaultState(this);
    }
}
