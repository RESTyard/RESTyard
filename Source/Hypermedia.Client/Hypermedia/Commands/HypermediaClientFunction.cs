using System;
using System.Threading.Tasks;

namespace Bluehands.Hypermedia.Client.Hypermedia.Commands
{
    public class HypermediaClientFunction<TResultType>
        : HypermediaClientCommandBase,
            IHypermediaClientFunction<TResultType>
        where TResultType : HypermediaClientObject
    {
        public HypermediaClientFunction()
        {
            this.HasParameters = false;
            this.HasResultLink = true;
        }
    }

    public class HypermediaClientFunction<TResultType, TParameters>
        : HypermediaClientCommandBase,
            IHypermediaClientFunction<TResultType, TParameters>
        where TResultType : HypermediaClientObject
    {
        public HypermediaClientFunction()
        {
            this.HasParameters = true;
            this.HasResultLink = true;
        }
    }
}