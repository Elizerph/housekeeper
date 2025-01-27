using HouseKeeper.Contexts;
using HouseKeeper.Models;

using Microsoft.EntityFrameworkCore;

namespace HouseKeeper.Core.States;
public class PickObservationAttributeState : DefaultState
{
    private readonly long _datasetId;
    private readonly Dictionary<int, long> _dimensionSelection;
    private readonly Dictionary<int, string> _observationAttributesValues;

    private static readonly SemaphoreSlim semaphoreSlim = new(1);

    public PickObservationAttributeState(State state, long datasetId, Dictionary<int, long> dimensionSelection, Dictionary<int, string> observationAttributesValues) 
        : base(state)
    {
        _datasetId = datasetId;
        _dimensionSelection = dimensionSelection;
        _observationAttributesValues = observationAttributesValues;
    }

    protected override async Task<IState> InputTextInner(string text)
    {
        await using var context = await ApplicationContextFactory.Create();
        var dataset = await context.Datasets
            .AsNoTracking()
            .Include(x => x.ObservationAttributes)
            .Where(x => x.Id == _datasetId)
            .FirstOrDefaultAsync();
        if (dataset != null)
        {
            var index = _observationAttributesValues.Count;
            _observationAttributesValues[index] = text;
            index++;
            if (index < dataset.ObservationAttributes.Count)
            {
                var observationAttribute = dataset.ObservationAttributes.ElementAt(index);
                await Dialog.Send($"Enter value for {observationAttribute.Name}:");
                return new PickObservationAttributeState(this, _datasetId, _dimensionSelection, _observationAttributesValues);
            }
            else
            {
                var timeseriesId = await GetTimeseriesId(context, _dimensionSelection.OrderBy(x => x.Key).Select(x => x.Value));
                var observation = new Observation
                { 
                    TimeseriesId = timeseriesId,
                    Timestamp = DateTime.UtcNow
                };
                await context.AddAsync(observation);
                await context.SaveChangesAsync();
                foreach (var pair in _observationAttributesValues)
                {
                    var attribute = dataset.ObservationAttributes.ElementAt(pair.Key);
                    var observationValue = new ObservationAttributeValue
                    {
                        AttributeId = attribute.Id,
                        ObservationId = observation.Id,
                        Value = pair.Value
                    };
                    await context.AddAsync(observationValue);
                }
                await context.SaveChangesAsync();
                await Dialog.Send("Observation saved");
                return new DefaultState(this);
            }
        }
        else
        {
            await Dialog.Send("Dataset does not exist");
            return new DefaultState(this);
        }
    }

    private async Task<long> GetTimeseriesId(ApplicationContext context, IEnumerable<long> elementIds)
    {
        var mnemonics = string.Join('.', elementIds);
        await semaphoreSlim.WaitAsync();
        try
        {
            var timeseries = await context.Timeseries
                .AsNoTracking()
                .Where(x => x.Mnemonics == mnemonics)
                .FirstOrDefaultAsync();
            if (timeseries == null)
            {
                var elements = new List<Element>();
                foreach (var elementId in elementIds)
                {
                    var element = await context.Elements
                        .Where(x => x.Id == elementId)
                        .FirstAsync();
                    elements.Add(element);
                }
                
                timeseries = new Timeseries
                {
                    DatasetId = _datasetId,
                    Mnemonics = mnemonics,
                    Elements = elements
                };
                await context.AddAsync(timeseries);
                await context.SaveChangesAsync();
            }
            return timeseries.Id;
        }
        catch
        {
            throw;
        }
        finally
        {
            semaphoreSlim.Release();
        }
    }
}
