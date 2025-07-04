namespace RESTyard.Extensions.Pagination;

/// <summary>
/// Defines a contract for deep copying instances.
/// </summary>
/// <typeparam name="TInstance"></typeparam>
public interface IDeepCopyable<out TInstance>
{
    /// <summary>
    /// Has clone semantics, but clone is a reserved method for C# records.
    /// </summary>
    TInstance DeepCopy();
}