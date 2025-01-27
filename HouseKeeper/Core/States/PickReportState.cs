
using Microsoft.EntityFrameworkCore;

using System.Linq;

namespace HouseKeeper.Core.States;
public class PickReportState : DefaultState
{
    public PickReportState(State state) 
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
                .Include(x => x.Dimensions)
                .Include(x => x.ObservationAttributes)
                .Where(x => x.Id == datasetId)
                .FirstOrDefaultAsync();
            if (dataset != null)
            {
                var header = dataset.Dimensions.OrderBy(x => x.Id).Select(x => x.Name)
                    .Concat(["Date"])
                    .Concat(dataset.ObservationAttributes.OrderBy(x => x.Id).Select(x => x.Name));
                var data = context.Timeseries
                    .AsNoTracking()
                    .Include(x => x.Elements)
                    .Include(x => x.Observations)
                        .ThenInclude(x => x.Values)
                    .Where(x => x.DatasetId == datasetId)
                    .AsAsyncEnumerable()
                    .SelectMany(x => x.Observations.ToAsyncEnumerable())
                    .Select(x => x.Timeseries.Elements.OrderBy(y => y.DimensionId).Select(y => y.Name)
                        .Concat([x.Timestamp.ToShortDateString()])
                        .Concat(x.Values.OrderBy(y => y.AttributeId).Select(y => y.Value)));
                await Dialog.SendCsvFile($"{dataset.Name} report", $"{dataset.Name}_{DateTime.UtcNow}.csv", new[] { header }.ToAsyncEnumerable().Concat(data));
                var timeseries = context.Timeseries
                    .AsNoTracking()
                    .Include(x => x.Observations)
                        .ThenInclude(x => x.Values)
                    .Where(x => x.DatasetId == datasetId)
                    .AsAsyncEnumerable();
                await foreach (var batch in timeseries.Buffer(2000))
                {
                    context.Timeseries.RemoveRange(batch);
                    await context.SaveChangesAsync();
                }
                return new DefaultState(this);
            }
            else
            {
                await Dialog.Send("Dataset does not exist");
                return new DefaultState(this);
            }
        }
        else
        {
            await Dialog.Send("Out of context");
            return new DefaultState(this);
        }
    }
}
