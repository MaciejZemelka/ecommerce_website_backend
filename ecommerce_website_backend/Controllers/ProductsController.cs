using ecommerce_website_backend.models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace ecommerce_website_backend.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ProductsDbContext _context;

        public ProductsController(ProductsDbContext context)
        {
            _context = context;
        }


        [HttpGet("Products")]
        public async Task<IActionResult> GetFilteredProducts(
        [FromQuery] int? size,
        [FromQuery] string? color,
        [FromQuery] string? gender,
        [FromQuery] string? height)
        {
            var query = _context.Products
                .Include(p => p.ProductVariants)
                    .ThenInclude(v => v.ProductImages)
                .Include(p => p.ProductVariants)
                    .ThenInclude(v => v.VariantSizes)
                    .ThenInclude(vs => vs.ShoeSize)
                .AsQueryable();

            // Filtrowanie na poziomie Product
            if (!string.IsNullOrEmpty(gender))
                query = query.Where(p => p.Gender == gender);

            if (!string.IsNullOrEmpty(height))
                query = query.Where(p => p.Height == height);

            // Filtrowanie na poziomie wariantów
            if (!string.IsNullOrEmpty(color))
                query = query.Where(p => p.ProductVariants.Any(v => v.Color == color));

            if (size.HasValue)
            {
                query = query.Where(p => p.ProductVariants.Any(v =>
                    v.VariantSizes.Any(vs => vs.ShoeSize.Size == size.Value)));
            }

            // Pobieranie produktów i mapowanie na obiekt wynikowy
            var products = await query
                .Select(p => new
                {
                    p.ProductId,
                    p.Name,
                    p.Description,
                    p.Category,
                    p.Price,
                    p.Gender,
                    p.Height,
                    Variants = p.ProductVariants
                        .GroupBy(v => v.Color)  // Grupowanie po kolorze
                        .Select(g => new
                        {
                            Color = g.Key,
                            Sizes = g.SelectMany(v => v.VariantSizes)
                                     .Select(vs => vs.ShoeSize.Size)
                                     .Distinct()
                                     .ToList(),
                            Discount = g.First().Discount,  // Zakładamy, że rabat jest taki sam dla wszystkich rozmiarów danego koloru
                            Images = g.SelectMany(v => v.ProductImages)
                                      .Select(pi => new
                                      {
                                          pi.Image1,
                                          pi.Image2,
                                          pi.Image3,
                                          pi.Image4
                                      })
                                      .Distinct()
                                      .ToList()
                        })
                        .ToList()
                })
                .ToListAsync();

            return Ok(products);
        }


        [HttpGet("Filters")]
        public async Task<IActionResult> GetAvailableFilters()
        {
         
            var sizes = await _context.ShoeSizes
                .Select(v => v.Size)
                .Distinct()
                .ToListAsync();

            var colors = await _context.ProductVariants
                .Select(v => v.Color)
                .Distinct()
                .ToListAsync();

            var genders = await _context.ProductVariants
                .Select(v => v.Product.Gender)
                .Distinct()
                .ToListAsync();

            var heights = await _context.ProductVariants
                .Select(v => v.Product.Height)
                .Distinct()
                .ToListAsync();


            var filters = new
            {
                Sizes = sizes,
                Colors = colors,
                Genders = genders,
                Heights = heights
            };

            return Ok(filters); 
        }

        [HttpGet("GetProduct")]
        public async Task<IActionResult> GetProduct(
            [FromQuery] int? ProductId,
            [FromQuery] string? color)
        {
            if (!ProductId.HasValue || string.IsNullOrEmpty(color))
            {
                return BadRequest("VariantId and color are required.");
            }

            var query = _context.Products
                .Include(p => p.ProductVariants)
                    .ThenInclude(v => v.ProductImages)
                .Include(p => p.ProductVariants)
                    .ThenInclude(v => v.VariantSizes)
                    .ThenInclude(vs => vs.ShoeSize)
                .AsQueryable();

            var product = await query
        .Where(p => p.ProductVariants.Any(v => v.ProductId == ProductId && v.Color == color))
        .Select(p => new
        {
            ProductId = p.ProductId,
            Name = p.Name,
            Price = p.Price,
            Variant = p.ProductVariants
                .Where(v => v.Color == color)
                .Select(v => new
                {
                    Color = v.Color,
                    Images = v.ProductImages.Select(pi => new
                    {
                        pi.Image1,
                        pi.Image2,
                        pi.Image3,
                        pi.Image4
                    }).ToList(),
                    Sizes = v.VariantSizes.Select(vs => vs.ShoeSize.Size).ToList()
                })
                .FirstOrDefault() 
        })
        .FirstOrDefaultAsync();

            if (product == null)
            {
                return NotFound("Product or variant not found.");
            }

            return Ok(product);
        }
    }

   

}
