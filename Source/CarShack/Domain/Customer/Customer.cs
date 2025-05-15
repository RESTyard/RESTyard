namespace CarShack.Domain.Customer
{
    // The domain object Customer.
    public class Customer
    {
        public int Age { get; set; }

        public int Id { get; set; }

        public string Name { get; set; }

        public Address Address { get; set; }

        public bool IsFavorite { get; set; }

        public Customer(int id, string name, int age, Address address, bool isFavorite)
        {
            Name = name;
            Id = id;
            Age = age;
            Address = address;
            IsFavorite = isFavorite;
        }
    }
}
