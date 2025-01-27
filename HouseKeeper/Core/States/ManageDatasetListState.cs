using Microsoft.EntityFrameworkCore;

using Newtonsoft.Json;

namespace HouseKeeper.Core.States;
public class ManageDatasetListState : DefaultState
{
    public ManageDatasetListState(State state)
        : base(state)
    {
    }

    public override async Task<IState> InputButton(int messageId, string buttonData)
    {
        await Dialog.ClearKeyboard(messageId);
        if (long.TryParse(buttonData, out var datasetId))
        {
            await using var context = await ApplicationContextFactory.Create();
            var dataset = await context.Datasets
                .AsNoTracking()
                .Where(x => x.Id == datasetId)
                .FirstOrDefaultAsync();
            if (dataset != null)
            {
                var buttons = new[]
                {
                    new MessageButton
                    { 
                        Label = "Export",
                        Data = "export"
                    },
                    new MessageButton
                    {
                        Label = "Rename",
                        Data = "rename"
                    },
                    new MessageButton
                    {
                        Label = "Dimensions",
                        Data = "dimensions"
                    },
                    new MessageButton
                    {
                        Label = "Observation attributes",
                        Data = "observation_attributes"
                    },
                    new MessageButton
                    {
                        Label = "Delete",
                        Data = "delete"
                    }
                };
                await Dialog.Send($"Manage {dataset.Name}:", buttons);
                return new ManageDatasetState(this, dataset.Id);
            }
            else
            {
                await Dialog.Send("Dataset does not exist");
                return new DefaultState(this);
            }
        }
        else
        {
            switch (buttonData)
            {
                case "create":
                    await Dialog.Send("Enter new dataset name:");
                    return new CreateDatasetState(this);
                case "import":
                    await Dialog.Send("Drop json file:");
                    return new ImportDatasetState(this);
                default:
                    await Dialog.Send("Out of context");
                    return new DefaultState(this);
            }
        }
    }
}
