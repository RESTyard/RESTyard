﻿using CarShack.Domain.Customer;
using WebApiHypermediaExtensionsCore.Exceptions;
using WebApiHypermediaExtensionsCore.Hypermedia;
using WebApiHypermediaExtensionsCore.Hypermedia.Attributes;
using WebApiHypermediaExtensionsCore.WebApi.RouteResolver;

namespace CarShack.Hypermedia.Customers
{
    [HypermediaObject(Title = "A Customer", Classes = new[] { "Customer" })]
    public class HypermediaCustomer : HypermediaObject
    {
        private readonly Customer customer;

        // Add actions:
        // Each ActionType must be unique and a corresponding route must exist so the formatter can look it up.
        // See the CustomerController.
        [HypermediaAction(Name = "CustomerMove", Title = "A Customer moved to a new location.")]
        public HypermediaActionCustomerMoveAction MoveAction { get; private set; }

        [HypermediaAction(Title = "Marks a Customer as a favorite buyer.")]
        public HypermediaActionCustomerMarkAsFavorite MarkAsFavoriteAction { get; private set; }

        // Hides the Property so it will not be pressent in the Hypermedia.
        [FormatterIgnoreHypermediaProperty]
        // Marks property so it is can be mapped to route parameters when creating links
        [RouteTemplateParameter]
        public int Id { get; set; }

        // Assigns an alternative name, so this stays constant even if property is renamed
        [HypermediaProperty(Name = "FullName")]
        public string Name { get; set; }
        
        public int Age { get; set; }

        public string Address { get; set; }

        public bool IsFavorite { get; set; }

        public HypermediaCustomer(Customer customer)
        {
            this.customer = customer;

            Name = customer.Name;
            Id = customer.Id;
            Age = customer.Age;
            IsFavorite = customer.IsFavorite;
            Address = customer.Address;

            MoveAction = new HypermediaActionCustomerMoveAction(CanMove, DoMove);
            MarkAsFavoriteAction = new HypermediaActionCustomerMarkAsFavorite (CanMarkAsFavorite, DoMarkAsFavorite);
        }

        private bool CanMarkAsFavorite()
        {
            return !IsFavorite;
        }

        private void DoMarkAsFavorite(FavoriteCustomer favoriteCustomer)
        {
            customer.IsFavorite = true;
            IsFavorite = customer.IsFavorite;
        }

        private bool CanMove()
        {
            return true;
        }

        private void DoMove(NewAddress newAddress)
        {
            // semantic validation is busyness logic
            if (string.IsNullOrEmpty(newAddress.Address))
            {
                throw new ActionParameterValidationException("New customer adress may not be null or empthy.");
            }

            // call busyness logic here
            customer.Address = newAddress.Address;
            Address = customer.Address;
        }
    }
}