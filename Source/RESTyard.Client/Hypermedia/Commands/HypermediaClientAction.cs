﻿using System;

namespace RESTyard.Client.Hypermedia.Commands
{
    public class HypermediaClientAction
        : HypermediaClientCommandBase, IHypermediaClientAction
    {
        public HypermediaClientAction()
        {
            this.HasParameters = false;
            this.HasResultLink = false;
        }
    }

    public class HypermediaClientAction<TParameters>
        : HypermediaClientCommandBase, IHypermediaClientAction<TParameters>
    {
        public HypermediaClientAction()
        {
            this.HasParameters = true;
            this.HasResultLink = false;
        }
    }
}