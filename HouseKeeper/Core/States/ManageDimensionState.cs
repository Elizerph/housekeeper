
using Microsoft.EntityFrameworkCore;

namespace HouseKeeper.Core.States;
public class ManageDimensionState : DefaultState
{
    private readonly long _dimensionId;

    public ManageDimensionState(State state, long dimensionId) 
        : base(state)
    {
        _dimensionId = dimensionId; 
    }

    public override async Task<IState> InputButton(int messageId, string buttonData)
    {
        await Dialog.ClearKeyboard(messageId);
        switch (buttonData)
        {
            case "rename":
                await Dialog.Send("Enter new name:");
                return new RenameDimensionState(this, _dimensionId);
            case "elements":
                {
                    await using var context = await ApplicationContextFactory.Create();
                    var dimension = await context.Dimensions
                        .AsNoTracking()
                        .Include(x => x.Elements)
                        .Where(x => x.Id == _dimensionId)
                        .FirstOrDefaultAsync();
                    if (dimension != null)
                    {
                        var buttons = dimension.Elements.Select(x => new MessageButton
                        {
                            Label = x.Name,
                            Data = x.Id.ToString()
                        }).ToList();
                        buttons.Add(new MessageButton
                        {
                            Label = "Create",
                            Data = "create"
                        });
                        await Dialog.Send($"Manage elements of {dimension.Name}:", buttons);
                        return new ManageElementListState(this, _dimensionId);
                    }
                    else
                    {
                        await Dialog.Send("Dataset does not exist");
                        return new DefaultState(this);
                    }
                }
            case "delete":
                {
                    await using var context = await ApplicationContextFactory.Create();
                    var dimension = await context.Dimensions
                        .AsNoTracking()
                        .Where(x => x.Id == _dimensionId)
                        .FirstOrDefaultAsync();
                    if (dimension != null)
                    {
                        context.Remove(dimension);
                        await context.SaveChangesAsync();
                        await Dialog.Send("Dimension deleted");
                        return new DefaultState(this);
                    }
                    else
                    {
                        await Dialog.Send("Dimension does not exist");
                        return new DefaultState(this);
                    }
                }
            default:
                await Dialog.Send("Out of context");
                return new DefaultState(this);
        }
    }
}
