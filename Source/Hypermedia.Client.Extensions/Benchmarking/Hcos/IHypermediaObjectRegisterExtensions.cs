using System.Linq;
using System.Reflection;
using Bluehands.Hypermedia.Client;
using RESTyard.Client;
using RESTyard.Client.Hypermedia;

namespace Benchmarking.Hcos
{
    public static class IHypermediaObjectRegisterExtensions
    {
        public static void Register<THco, TProgressHco>(
            this IHypermediaObjectRegister register)
            where THco : HypermediaClientObject
            where TProgressHco : ProgressHco<THco>
        {
            register.Register<THco>();
            register.Register<TProgressHco>();
        }

        public static void RegisterAllClassesDeriving<TClass>(
            this IHypermediaObjectRegister register,
            Assembly assemblyHint = null)
            where TClass : HypermediaClientObject
        {
            var assembly = assemblyHint ?? Assembly.GetExecutingAssembly();
            foreach (var type in assembly
                         .GetTypes()
                         .Where(t => typeof(TClass).IsAssignableFrom(t)))
            {
                register.Register(type);
            }
        }
    }
}