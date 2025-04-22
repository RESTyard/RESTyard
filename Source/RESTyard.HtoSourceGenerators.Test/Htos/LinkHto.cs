using RESTyard.HtoSourceGenerators.Attributes;
using RESTyard.HtoSourceGenerators.Model;

namespace RESTyard.HtoSourceGenerators.Test.Htos;

[HypermediaObject(Title = "Only contains links")]
public class LinkHto
{
    // todo how to mark "required", nullable ?
    [Relation("Partner")]
    [HypermediaLink(Title="My Title")] // optional
    public Link<LinkTargetHto> Partner { get; set; }
    
    [Relation("Parent")] // same relation different media type
    public Link<LinkTargetHto> PartnerAsXML { get; set; }
    
    [Relation("Child", "Firstborn")]
    public Link<LinkTargetHto> PartnerFromKeys { get; set; }
    
    [Relation("Next")]
    public Link<LinkTargetHto> QueryResultLink { get; set; }
    
    // external route not managed by RESTyard
    // todo
    // [Relation("School")]
    // public ExternalLink Partner4 { get; set; } = ExternalLink.To(uri, classes, mediatype);
    
    // internal route not managed by RESTyard
    // tod
    //[Relation("Sister")]
    // maybe dont provide this, let user build URL by his choosing
    //public InternalLink Partner5 { get; set; } = InternalLink.To(controller, action, classes, mediatype);

    public LinkHto(LinkTargetHto linkTargetHto)
    {
        // todo
        // Partner = Link.To(linkTargetHto);
        // PartnerAsXML = Link.To(linkTargetHto, mediatype.xml); // with non default media type
        // PartnerFromKeys = Link.To(LinkTargetHto.CreateKeys(linkTargetHto.key1, linkTargetHto.key2));
        // QueryResultLink  = Link.To(LinkQueryTargetHto.CreateQuery(queryObject));
        
        // QueryResultLink  = Link.ToLinkQueryTargetHto(queryObject);
        // QueryResultLink  = Link.ToLinkQueryTargetHto(keys, queryObject);
        // QueryResultLink  = Link.ToLinkQueryTargetHto(keys);
    }
}

[HypermediaObject(Title = "Target for links")]
public class LinkTargetHto;

//Todo
//[HypermediaObject(Title = "Target for queries", QueryObjectType = typeof(LinkQueryTargetHtoQuery))] 
// all objects can have query string, so maybe add here, but then onnly for get
// also allow default query definition (+modification of default at runtime?)
public class LinkQueryTargetHto;


