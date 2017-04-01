namespace HypermediaClient.Hypermedia.Commands
{
    public class HypermediaCommandResult
    {
        public bool Success { set; get; }
    }

    public class HypermediaFunctionResult<TResult> : HypermediaCommandResult where TResult : HypermediaClientObject
    {
        // todo return code/enum like 200, 404, 409
        public MandatoryHypermediaLink<TResult> ResultLocation { set; get; } = new MandatoryHypermediaLink<TResult>();
    }
}