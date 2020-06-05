using FunicularSwitch;

namespace Bluehands.Hypermedia.Model
{
    public class Action
    {
        public string Name { get; }
        public string ActionName { get; }
        public string Title { get; }
        public Option<TypeDescriptor> ParameterType { get; }
        public Option<TypeDescriptor> ReturnType { get; }

        public Action(string name, string actionName, string title, Option<TypeDescriptor> parameterType, Option<TypeDescriptor> returnType)
        {
            Name = name;
            ActionName = actionName;
            Title = title;
            ParameterType = parameterType;
            ReturnType = returnType;
        }

        public override string ToString() => $"{ReturnType.Match(r => r.ToString(), () => "void")} {ActionName}";
    }
}