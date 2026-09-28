using IdentityDemoApp.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IdentityDemoApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }


        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.VendorProfile)
                .Where(p =>
                    p.IsAvailable &&
                    p.ModerationStatus == "Approved")
                .OrderByDescending(p => p.CreatedAt)
                .Take(8)
                .ToListAsync();

            var categories = await _context.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.CategoryName)
                .Take(8)
                .ToListAsync();

            var approvedReviews = await _context.Reviews
                .AsNoTracking()
                .Include(r => r.Product)
                .Include(r => r.CustomerProfile)
                .Where(r =>
                    r.IsApproved &&
                    r.Product != null &&
                    r.Product.IsAvailable &&
                    r.Product.ModerationStatus == "Approved")
                .OrderByDescending(r => r.CreatedAt)
                .Take(6)
                .ToListAsync();

            ViewBag.FeaturedProducts = products;
            ViewBag.FeaturedCategories = categories;
            ViewBag.ApprovedReviews = approvedReviews;

            ViewBag.TotalProducts =
                await _context.Products
                    .CountAsync(p =>
                        p.IsAvailable &&
                        p.ModerationStatus == "Approved");

            ViewBag.TotalVendors =
                await _context.VendorProfiles
                    .CountAsync(v => v.IsApproved);

            ViewBag.TotalCategories =
                await _context.Categories
                    .CountAsync(c => c.IsActive);

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Products(
            string? search,
            int? categoryId)
        {
            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.VendorProfile)
                .Where(p =>
                    p.IsAvailable &&
                    p.ModerationStatus == "Approved")
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(p =>
                    p.ProductName.Contains(search) ||
                    p.Description.Contains(search) ||
                    (p.VendorProfile != null &&
                     p.VendorProfile.FarmName.Contains(search)) ||
                    (p.Category != null &&
                     p.Category.CategoryName.Contains(search)));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(p =>
                    p.CategoryId == categoryId.Value);
            }

            var products = await query
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            var categories = await _context.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.CategoryName)
                .ToListAsync();

            ViewBag.Categories = categories;
            ViewBag.Search = search;
            ViewBag.CategoryId = categoryId;

            return View(products);
        }

        [HttpGet]
        public async Task<IActionResult> ProductDetails(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.VendorProfile)
                .FirstOrDefaultAsync(p =>
                    p.ProductId == id &&
                    p.IsAvailable &&
                    p.ModerationStatus == "Approved");

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        [HttpGet]
        public async Task<IActionResult> Categories(string? search)
        {
            var query = _context.Categories
                .Where(c => c.IsActive)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(c =>
                    c.CategoryName.Contains(search) ||
                    (c.Description != null &&
                     c.Description.Contains(search)));
            }

            var categories = await query
                .OrderBy(c => c.CategoryName)
                .ToListAsync();

            ViewBag.Search = search;

            return View(categories);
        }

        [HttpGet]
        public async Task<IActionResult> Vendors(string? search)
        {
            var query = _context.VendorProfiles
                .Include(v => v.User)
                .Where(v => v.IsApproved)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(v =>
                    v.FarmName.Contains(search) ||
                    v.Address.Contains(search) ||
                    v.City.Contains(search) ||
                    v.Province.Contains(search) ||
                    (v.User != null &&
                     v.User.FullName.Contains(search)));
            }

            var vendors = await query
                .OrderBy(v => v.FarmName)
                .ToListAsync();

            ViewBag.Search = search;

            return View(vendors);
        }

        [HttpGet]
        public IActionResult About()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Contact()
        {
            return View();
        }
    }
}