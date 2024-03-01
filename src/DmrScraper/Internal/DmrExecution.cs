namespace DmrScraper.Internal;

internal sealed class DmrExecution(int executionId, int actionId)
{
    private const string ExecutionIdPrefix = "e";
    private const string ActionIdPrefix = "s";

    private int _actionId = actionId;

    public int ExecutionId { get; } = executionId;
    public int ActionId
    {
        get
        {
            int actionId = _actionId;
            Interlocked.Increment(ref _actionId);
            return actionId;
        }
    }

    public override string ToString() 
        => $"{ExecutionIdPrefix}{ExecutionId}{ActionIdPrefix}{ActionId}";

    private const string ExecutionParameter = "execution=";
    private const char QuerySeparator = '&';

    public static DmrExecution FromUri(ReadOnlySpan<char> url)
    {
        int executionParamIdx = url.IndexOf(ExecutionParameter);
        if (executionParamIdx == -1)
            throw new ArgumentException("URL does not contain the execution parameter", nameof(url));

        url = url.Slice(executionParamIdx + ExecutionParameter.Length);

        int querySeparatorIdx = url.IndexOf(QuerySeparator);

        ReadOnlySpan<char> execution = querySeparatorIdx == -1
            ? url
            : url.Slice(0, querySeparatorIdx);

        return FromExecution(execution);
    }

    public static DmrExecution FromExecution(ReadOnlySpan<char> execution)
    {
        int executionIdIdx = execution.IndexOf(ExecutionIdPrefix) + ExecutionIdPrefix.Length;
        int actionIdIdx = execution.IndexOf(ActionIdPrefix) + ActionIdPrefix.Length;

        ReadOnlySpan<char> executionIdSpan = execution.Slice(executionIdIdx, actionIdIdx - executionIdIdx - ActionIdPrefix.Length);
        ReadOnlySpan<char> actionIdSpan = execution.Slice(actionIdIdx);

        int executionId = int.Parse(executionIdSpan);
        int actionId = int.Parse(actionIdSpan);

        return new(executionId, actionId);
    }
}
