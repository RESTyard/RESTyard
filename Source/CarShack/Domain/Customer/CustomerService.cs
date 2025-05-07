using System;
using System.Collections.Generic;
using CarShack.Util;

namespace CarShack.Domain.Customer
{
    public static class CustomerService
    {
        private static readonly Random random = new Random(33);
        private static readonly NameGenerator nameGenerator = new NameGenerator(12);
        private static readonly AdressGenerator adressGenerator = new AdressGenerator(15);

        private static int InternalIdCounter;

        public static Customer CreateRandomCustomer(bool? isFavorite = null)
        {
            var result = new Customer(InternalIdCounter, nameGenerator.GenerateNext(), random.Next(14, 99), adressGenerator.GenerateNext(), isFavorite: isFavorite ?? random.Next(0, 2) == 0);
            InternalIdCounter++;
            return result;

        }

        public static List<Customer> GenerateRandomCustomersList(int count)
        {
            var result = new List<Customer>();

            for (var id = 0; id < count; id++)
            {
                result.Add(CreateRandomCustomer());
            }

            return result;
        }
    }
}
