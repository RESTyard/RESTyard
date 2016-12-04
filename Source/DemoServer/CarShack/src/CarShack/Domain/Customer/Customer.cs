namespace CarShack.Domain.Customer
{
    // The domain object Customer.
    public class Customer
    {
        public int Age { get; set; }

        public int Id { get; set; }

        public string Name { get; set; }

        public string Address { get; set; }

        public bool IsFavorite { get; set; }

        public Customer(int id, string name, int age, string adress, bool isFavorite)
        {
            Name = name;
            Id = id;
            Age = age;
            Address = adress;
            IsFavorite = isFavorite;
        }
    }
}
