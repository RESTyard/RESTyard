namespace HypermediaClient.ParameterSerializer
{
    public interface IParameterSerializer
    {
        string SerializeParameterObject(string parameterObjectName, object parameterObject);
    }
}