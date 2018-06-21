using System;

namespace WebApi.HypermediaExtensions
{
    public static class ApplicationModelSingleton
    {
        static ApplicationModel _applicationModel;

        public static ApplicationModel Instance
        {
            get
            {
                if(_applicationModel == null) {throw new InvalidOperationException("Application model instance is null. Make sure to initialize ApplicationModel first");}
                return _applicationModel;
            }
            set => _applicationModel = value;
        }
    }
}
