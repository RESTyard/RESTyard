using RESTyard.HtoSourceGenerators.Attributes;
using RESTyard.HtoSourceGenerators.Model;

namespace RESTyard.HtoSourceGenerators.Test.Htos;

[HypermediaObject(Title = "Only contains sub entities")]
public class WithEmbeddedHto
{
    // todo how to mark "required", nullable ?
    [Relation("EmbeddedDirect")]
    public EmbeddedHto EmbeddedDirect { get; set; }
    
    [Relation("EmbeddedDirectList","listitem")]
    public List<EmbeddedHto> EmbeddedListDirect { get; set; }
    
    [Relation("rel1")]
    public EmbeddedLink<EmbeddedHto> EmbeddedAsLink { get; set; }
    
    [Relation("rel2","listitem")]
    public List<EmbeddedLink<EmbeddedHto>> EmbeddedAsLinkList { get; set; }
    
    [Relation("rel2")]
    public EmbeddedLink<QueryResultHto> QueryLink { get; set; }

    public WithEmbeddedHto(EmbeddedHto hto)
    {
        EmbeddedDirect = hto;
        EmbeddedListDirect = [hto, hto];
        // todo keys AND query at the same time?
        // EmbeddedAsLink =  Link.To(EmbeddedHto.CreateLink(keys1, key2));
        // EmbeddedAsLinkList = [Link.To(EmbeddedHto.CreateLink(keys1, key2))];
        // // with non default media type?
        // QueryLink = Link.To(QueryResultHto.CreateQuery(queryparameter1, queryparameter2));
    }
}

[HypermediaObject(Title = "Is embedded")]
public class EmbeddedHto;

[HypermediaObject(Title = "Is embedded")]
public class QueryResultHto;





