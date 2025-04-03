﻿namespace CatalogChallengeNet8.Application.Models
{
    public class ProductModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string CategoryCode { get; set; } = string.Empty;
    }
}
