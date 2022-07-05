using System;
using System.Collections.Generic;

namespace WebApi.HypermediaExtensions.Test.Hypermedia
{
    [Hto(NoAutoSelfLink = true)] // for serializer to know it is an hto, maybe not needed because we should tell by media type, use [Produces("application/vnd.siren+json")]?
    public class TestHto
    {
        [HtoTitle] // this might be used dynamic, also ok on class if static (recommended) but only one of them, not serialized as property
        public string Title { get; set; }

        [HtoClasses] // this might be used dynamic, also ok on class if static (recommended) but only one of them, not serialized as property
        public IReadOnlyCollection<string> Classes { get; set; }

        // only use route names so we can get rid of all the special attributes in the controllers, no parameter specification or return type here needed
        [HypermediaActionNext(Title = "This is a Action with no return", Name = "MyVoidAction", RouteName = "ActionRouteName")]
        [Mandatory]
        public HypermediaAction2 SomeAction { get; set; } = HypermediaAction2.Available();

        // check in serialization that relation attribute is specified, if attribute and rel merge
        // recommended: better for analysing because it is static
        [Relations("Rel1", "Rel2")]
        [Mandatory]
        LinkTo<CustomerHto> MyCustomer1 = new LinkTo<CustomerHto>(new CustomerHto());

        [MandatoryItems(2)]
        LinkToMany<CustomerHto> MyCustomers = new LinkToMany<CustomerHto>(new List<CustomerHto> { new CustomerHto(), new CustomerHto() });

        LinkTo<CustomerHto> MyCustomer2 = new LinkTo<CustomerHto>("Rel1", new CustomerHto());
        
        LinkTo<CustomerHto> MyCustomers2 = new LinkTo<CustomerHto>(new List<string> { "Rel1", "Rel2" }, new CustomerHto());

        // same as for LinkTo/LinkToMany only other wrapper class with query object, check at startup that query type matches route parameters
        LinkToQuery<CustomerQueryResultHto, CustomerQuery> MyCustomer23 = new LinkToQuery<CustomerQueryResultHto, CustomerQuery>("Rel1", new CustomerQuery(), new { key = 1 });

        LinkByKeys<CustomerHto> MyCustomer4 = new LinkByKeys<CustomerHto>("Rel1", new { key = 1 });

        LinkToExtern MyCustomer5 = new LinkToExtern("rel1", "www.example.com", "class1");

        // same as for LinkTo/LinkToMany only other wrapper class
        Embedded<CustomerHto> EmbeddedCustomer = new Embedded<CustomerHto>("Rel1", new CustomerHto());

        EmbeddedMany<CustomerHto> EmbeddedCustomers = new EmbeddedMany<CustomerHto>("Rel1", new List<CustomerHto> { new CustomerHto() });
    }

    public class CustomerHto
    {
    }

    public class CustomerQueryResultHto
    {
    }

    public class CustomerQuery
    {
    }

    public class LinkTo<T> : RelationsBase
    {
        public readonly T Hto;

        public LinkTo(T hto)
        {
            this.Hto = hto;
        }

        public LinkTo(string relation, T hto) : base(relation)
        {
            this.Hto = hto;
        }

        public LinkTo(List<string> relations, T hto) : base(relations)
        {
            this.Hto = hto;
        }
    }

    public class LinkToMany<T> : RelationsBase
    {
        public readonly IReadOnlyCollection<T> Htos;

        public LinkToMany(IReadOnlyCollection<T> htos)
        {
            Htos = htos;
        }

        public LinkToMany(string relation, IReadOnlyCollection<T> htos) : base(relation)
        {
            Htos = htos;
        }

        public LinkToMany(List<string> relations, IReadOnlyCollection<T> htos) : base(relations)
        {
            Htos = htos;
        }
    }

    public class LinkByKeys<T> : RelationsBase
    {
        public LinkByKeys(object keys)
        {
        }

        public LinkByKeys(string relation, object keys) : base(relation)
        {
        }

        public LinkByKeys(List<string> relations, object keys) : base(relations)
        {
        }
    }

    public class LinkToQuery<THto, TQuery> : RelationsBase
    {
        public LinkToQuery(string relation, TQuery customerQuery, object keys = null) : base(relation)
        {
            throw new NotImplementedException();
        }
        // todo more ctors
    }

    public abstract class RelationsBase
    {
        private readonly List<string> relations;

        public RelationsBase(List<string> relations)
        {
            this.relations = relations;
        }

        public RelationsBase(string relations)
        {
            this.relations = new List<string> { relations };
        }

        public RelationsBase()
        {
            this.relations = new List<string>();
        }
    }

    public class EmbeddedMany<T> : RelationsBase
    {
        public EmbeddedMany(string relation, List<CustomerHto> customerHtos) : base(relation)
        {
            throw new NotImplementedException();
        }
        // todo more ctors
    }

    public class Embedded<T> : RelationsBase
    {
        public Embedded(string relation, CustomerHto customerHto) : base(relation)
        {
            throw new NotImplementedException();
        }

        // todo more ctors
    }

    public class LinkToExtern : RelationsBase
    {
        public LinkToExtern(string relation, string uri, IEnumerable<string> classes) : base(relation)
        {
        }

        public LinkToExtern(List<string> relations, string uri, IEnumerable<string> classes) : base(relations)
        {
        }

        public LinkToExtern(string relation, string uri, string @class) : base(relation)
        {
        }

        public LinkToExtern(List<string> relations, string uri, string @class) : base(relations)
        {
        }
    }

    // share with client project
    public class MandatoryItemsAttribute : Attribute
    {
        public MandatoryItemsAttribute(int mandatoryItemCount)
        {
            throw new NotImplementedException();
        }
    }

    // share with client project
    public class RelationsAttribute : Attribute
    {
        public RelationsAttribute(string rel1, string rel2)
        {
            throw new NotImplementedException();
        }
    }

    // share with client project
    public class MandatoryAttribute : Attribute
    {
    }

    public class HtoClassesAttribute : Attribute
    {
    }

    public class HtoTitleAttribute : Attribute
    {
    }

    public class HtoAttribute : Attribute
    {
        public bool NoAutoSelfLink { get; set; } = false;
    }
    
    public class HypermediaActionNextAttribute : Attribute
    {
        public string Title { get; set; }
        public string Name { get; set; }
        
        public string RouteName { get; set; }
    }
    
    // maybe we dont need all those different action
    
    /// <summary>
    ///  Action that does not return anything and has no parameter
    /// </summary>
    public class HypermediaAction2 : HypermediaOperationBase
    {
        public static HypermediaAction2 Available()
        {
            return new HypermediaAction2 {IsAvailable = true};
        }

        public static HypermediaAction2 NotAvailable()
        {
            return new HypermediaAction2();
        }
    }

    /// <summary>
    ///  Action that does not return anything and has a Parameter object
    /// </summary>
    public class HypermediaAction2<Paramerter> : HypermediaOperationBase
    {
        public static HypermediaAction2<Paramerter> Available()
        {
            return new HypermediaAction2<Paramerter> { IsAvailable = true };
        }

        public static HypermediaAction2<Paramerter> NotAvailable()
        {
            return new HypermediaAction2<Paramerter>();
        }
    }

    /// <summary>
    ///  Action that does have a Return and Parameter object
    /// </summary>
    public class HypermediaFunction2<Paramerter, Return> : HypermediaOperationBase
    {
        public static HypermediaFunction2<Paramerter, Return> Available()
        {
            return new HypermediaFunction2<Paramerter, Return> { IsAvailable = true };
        }

        public static HypermediaFunction2<Paramerter, Return> NotAvailable()
        {
            return new HypermediaFunction2<Paramerter, Return>();
        }
    }

    /// <summary>
    ///  Action that does have a Return
    /// </summary>
    public class HypermediaFunction2<Return> : HypermediaOperationBase
    {
        public static HypermediaFunction2<Return> Available()
        {
            return new HypermediaFunction2<Return> { IsAvailable = true };
        }

        public static HypermediaFunction2<Return> NotAvailable()
        {
            return new HypermediaFunction2<Return>();
        }
    }

    public abstract class HypermediaOperationBase
    {
        public bool IsAvailable { get; protected set; } = false;
    }
}