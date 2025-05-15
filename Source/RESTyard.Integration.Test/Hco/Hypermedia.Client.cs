namespace RESTyard.Integration.Test.Hco;

public partial record MarkAsFavoriteParameters
{
    public static MarkAsFavoriteParameters FromCustomer(HypermediaCustomerHco customer)
        => new(customer.Self.Uri!);
}