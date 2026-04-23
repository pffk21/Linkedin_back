using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Project_X_Data.Models.Api
{
    public class ApiProductFormModel
    {
        [FromForm(Name = "product-group-id")]
        [Required(ErrorMessage = "GroupId is required")]
        public string GroupId { get; set; } = null!;

        [FromForm(Name = "product-name")]
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name length must be <= 100 characters")]
        public string Name { get; set; } = null!;

        [FromForm(Name = "product-description")]
        [StringLength(500, ErrorMessage = "Description length must be <= 500 characters")]
        public string? Description { get; set; }

        [FromForm(Name = "product-slug")]
        [RegularExpression(@"^[a-z0-9-]+$", ErrorMessage = "Slug can contain only lowercase letters, digits and '-'")]
        public String? Slug { get; set; }

        [FromForm(Name = "product-img")]
        public IFormFile? Image { get; set; }

        [FromForm(Name = "product-price")]
        [Range(0.01, 100000, ErrorMessage = "Price must be greater than 0")]
        public double Price { get; set; }

        [FromForm(Name = "product-stock")]
        [Range(0, int.MaxValue, ErrorMessage = "Stock cannot be negative")]
        public int Stock { get; set; }
    }
}
