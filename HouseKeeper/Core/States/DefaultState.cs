using Microsoft.EntityFrameworkCore;

namespace HouseKeeper.Core.States;
public class DefaultState : State
{
    public DefaultState(State state)
        : base(state)
    {
    }

    public override async Task<IState> InputButton(int messageId, string buttonData)
    {
        await Dialog.Send("Out of context");
        return this;
    }

    public override async Task<IState> InputDocument(string fileId)
    {
        await Dialog.Send("Out of context");
        return this;
    }

    public override async Task<IState> InputText(string text)
    {
        switch (text)
        {
            case "/manage":
                {
                    await using var context = await ApplicationContextFactory.Create();
                    var datasets = await context.Datasets
                        .AsNoTracking()
                        .Where(x => x.OwnerId == Dialog.UserId)
                        .ToListAsync();

                    var buttons = datasets.Select(x =>
                        new MessageButton
                        {
                            Label = x.Name,
                            Data = x.Id.ToString()
                        }).ToList();
                    buttons.AddRange([
                        new MessageButton
                        {
                            Label = "Create",
                            Data = "create"
                        },
                        new MessageButton
                        { 
                            Label = "Import",
                            Data = "import"
                        }
                    ]);
                    await Dialog.Send("Datasets:", buttons);
                    return new ManageDatasetListState(this);
                }
            case "/observation":
                {
                    await using var context = await ApplicationContextFactory.Create();
                    var datasets = await context.Datasets
                        .AsNoTracking()
                        .Where(x => x.OwnerId == Dialog.UserId)
                        .ToListAsync();

                    var buttons = datasets.Select(x => new MessageButton
                    {
                        Label = x.Name,
                        Data = x.Id.ToString()
                    }).ToList();
                    await Dialog.Send("Pick dataset for observation:", buttons);
                    return new PickDatasetState(this);
                }
            case "/report":
                {
                    await using var context = await ApplicationContextFactory.Create();
                    var datasets = await context.Datasets
                        .AsNoTracking()
                        .Where(x => x.OwnerId == Dialog.UserId)
                        .ToListAsync();

                    var buttons = datasets.Select(x => new MessageButton
                    {
                        Label = x.Name,
                        Data = x.Id.ToString()
                    }).ToList();
                    await Dialog.Send("Pick dataset for report:", buttons);
                    return new PickReportState(this);
                }
            default:
                return await InputTextInner(text);
        }
    }

    protected virtual async Task<IState> InputTextInner(string text)
    {
        return this;
    }
}
