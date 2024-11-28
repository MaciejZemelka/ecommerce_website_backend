﻿namespace ecommerce_website_backend.models
{
    public class UpdatedAddress
    {
        public int AddressId { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string StreetName { get; set; }
        public string HouseNumber { get; set; }
        public string ApartmentNumber { get; set; }
        public string PostalCode { get; set; }
    }
}