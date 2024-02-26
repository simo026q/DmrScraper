namespace DmrScraper.Internal;

internal struct DmrExecution
{
    private const string ExecutionIdPrefix = "e";
    private const string ActionIdPrefix = "s";

    public int ExecutionId { get; }
    public int ActionId { get; private set; }

    public DmrExecution(int executionId, int actionId)
    {
        ExecutionId = executionId;
        ActionId = actionId;
    }

    public void IncrementActionId()
    {
        ActionId++;
    }

    public override readonly string ToString()
    {
        return $"{ExecutionIdPrefix}{ExecutionId}{ActionIdPrefix}{ActionId}";
    }

    private const string ExecutionParameter = "execution=";
    private const char QuerySeparator = '&';

    public static DmrExecution FromUri(ReadOnlySpan<char> url)
    {
        var executionParamIdx = url.IndexOf(ExecutionParameter);
        if (executionParamIdx == -1)
            throw new ArgumentException("URL does not contain the execution parameter", nameof(url));

        url = url.Slice(executionParamIdx + ExecutionParameter.Length);

        var querySeparatorIdx = url.IndexOf(QuerySeparator);

        var execution = querySeparatorIdx == -1
            ? url
            : url.Slice(0, querySeparatorIdx);

        return FromExecution(execution);
    }

    public static DmrExecution FromExecution(ReadOnlySpan<char> execution)
    {
        var executionIdIdx = execution.IndexOf(ExecutionIdPrefix) + ExecutionIdPrefix.Length;
        var actionIdIdx = execution.IndexOf(ActionIdPrefix) + ActionIdPrefix.Length;

        var executionIdSpan = execution.Slice(executionIdIdx, actionIdIdx - executionIdIdx - ActionIdPrefix.Length);
        var actionIdSpan = execution.Slice(actionIdIdx);

        var executionId = int.Parse(executionIdSpan);
        var actionId = int.Parse(actionIdSpan);

        return new(executionId, actionId);
    }
}
