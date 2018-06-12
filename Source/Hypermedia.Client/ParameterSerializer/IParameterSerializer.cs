namespace Hypermedia.Client.ParameterSerializer
{
    public interface IParameterSerializer
    {
        string SerializeParameterObject(string parameterObjectName, object parameterObject);
    }
}