namespace DmrScraper.Internal;

internal struct DmrExecution
{
    private const string ExecutionIdPrefix = "e";
    private const string ActionIdPrefix = "s";

    private const string ExecutionParameter = "execution=";
    private const char QuerySeparator = '&';

    public int ExecutionId { get; }
    public int ActionId { get; private set; }

    public DmrExecution(int executionId, int actionId)
    {
        ExecutionId = executionId;
        ActionId = actionId;
    }

    public DmrExecution(string execution)
    {
        var match = InternalRegex.GetDmrExecutionRegex().Match(execution);
        ExecutionId = int.Parse(match.Groups[ExecutionIdPrefix].Value);
        ActionId = int.Parse(match.Groups[ActionIdPrefix].Value);
    }

    public void IncrementActionId()
    {
        ActionId++;
    }

    public override string ToString()
    {
        return $"{ExecutionIdPrefix}{ExecutionId}{ActionIdPrefix}{ActionId}";
    }

    public static DmrExecution FromUri(ReadOnlySpan<char> url)
    {
        var executionParamIdx = url.IndexOf(ExecutionParameter);

        url = url.Slice(executionParamIdx + ExecutionParameter.Length);

        var querySeparatorIdx = url.IndexOf(QuerySeparator);

        var execution = url.Slice(0, querySeparatorIdx).ToString();

        return new DmrExecution(execution);
    }
}
