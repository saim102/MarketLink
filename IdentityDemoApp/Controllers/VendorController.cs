using IdentityDemoApp.Data;
using IdentityDemoApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IdentityDemoApp.Models.ViewModels;


namespace IdentityDemoApp.Controllers
{
    [Authorize(Roles = "Vendor")]
    public class VendorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public VendorController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Dashboard()
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var vendorProfile = await _context.VendorProfiles
                .FirstOrDefaultAsync(v => v.UserId == userId);

            if (vendorProfile == null)
            {
                TempData["Error"] =
                    "Vendor profile could not be found.";

                return View();
            }

            var vendorProfileId = vendorProfile.VendorProfileId;

         
            var totalProducts = await _context.Products
                .CountAsync(p => p.VendorProfileId == vendorProfileId);

            var availableProducts = await _context.Products
                .CountAsync(p =>
                    p.VendorProfileId == vendorProfileId &&
                    p.IsAvailable);

            var pendingProducts = await _context.Products
                .CountAsync(p =>
                    p.VendorProfileId == vendorProfileId &&
                    p.ModerationStatus == "Pending");

            var approvedProducts = await _context.Products
                .CountAsync(p =>
                    p.VendorProfileId == vendorProfileId &&
                    p.ModerationStatus == "Approved");

          
            var totalWeeklyStock = await _context.WeeklyStocks
                .CountAsync(w =>
                    w.VendorProfileId == vendorProfileId);

          
            var totalPickupSlots = await _context.PickupSlots
                .CountAsync(p =>
                    p.VendorProfileId == vendorProfileId);

            var availablePickupSlots = await _context.PickupSlots
                .CountAsync(p =>
                    p.VendorProfileId == vendorProfileId &&
                    p.IsAvailable &&
                    p.PickupDate >= DateTime.Today);

          
            var vendorOrders = _context.Orders
                .Where(o => o.OrderItems
                    .Any(i => i.Product!.VendorProfileId == vendorProfileId));

            var totalOrders = await vendorOrders.CountAsync();

            var pendingOrders = await vendorOrders
                .CountAsync(o => o.Status == "Pending");

            var completedOrders = await vendorOrders
                .CountAsync(o => o.Status == "Completed");

           
            var totalSales = await _context.OrderItems
                .Where(i =>
                    i.Product!.VendorProfileId == vendorProfileId &&
                    i.Order!.Status == "Completed")
                .SumAsync(i => (decimal?)i.TotalPrice) ?? 0m;

          
            var totalReviews = await _context.Reviews
                .CountAsync(r =>
                    r.Product!.VendorProfileId == vendorProfileId &&
                    r.IsApproved);

            var averageRating = await _context.Reviews
                .Where(r =>
                    r.Product!.VendorProfileId == vendorProfileId &&
                    r.IsApproved)
                .Select(r => (double?)r.Rating)
                .AverageAsync() ?? 0;

          
            var upcomingPickupSlots = await _context.PickupSlots
                .Where(p =>
                    p.VendorProfileId == vendorProfileId &&
                    p.PickupDate >= DateTime.Today)
                .OrderBy(p => p.PickupDate)
                .ThenBy(p => p.StartTime)
                .Take(5)
                .ToListAsync();

            var model = new VendorDashboardViewModel
            {
                VendorProfileId = vendorProfile.VendorProfileId,

                FarmName = vendorProfile.FarmName,
                City = vendorProfile.City,
                Province = vendorProfile.Province,
                FarmDescription = vendorProfile.FarmDescription,
                IsApproved = vendorProfile.IsApproved,

                TotalProducts = totalProducts,
                AvailableProducts = availableProducts,
                PendingProducts = pendingProducts,
                ApprovedProducts = approvedProducts,

                TotalWeeklyStock = totalWeeklyStock,

                TotalPickupSlots = totalPickupSlots,
                AvailablePickupSlots = availablePickupSlots,

                TotalOrders = totalOrders,
                PendingOrders = pendingOrders,
                CompletedOrders = completedOrders,

                TotalSales = totalSales,

                TotalReviews = totalReviews,
                AverageRating = averageRating,

                UpcomingPickupSlots = upcomingPickupSlots
            };

            return View(model);
        }

       

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var vendorProfile = await _context.VendorProfiles
                .FirstOrDefaultAsync(v => v.UserId == userId);

            if (vendorProfile == null)
            {
                TempData["Error"] =
                    "Vendor profile could not be found.";

                return RedirectToAction(nameof(Dashboard));
            }

            return View(vendorProfile);
        }

        

        [HttpGet]
        public async Task<IActionResult> ProfileCreate()
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

       
            var existingProfile = await _context.VendorProfiles
                .FirstOrDefaultAsync(v => v.UserId == userId);

            if (existingProfile != null)
            {
                return RedirectToAction(nameof(Profile));
            }

            return View();
        }


       

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProfileCreate(VendorProfile model)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

         
            var existingProfile = await _context.VendorProfiles
                .FirstOrDefaultAsync(v => v.UserId == userId);

            if (existingProfile != null)
            {
                TempData["Error"] = "Your vendor profile already exists.";

                return RedirectToAction(nameof(Profile));
            }

          
            ModelState.Remove(nameof(VendorProfile.UserId));

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var vendorProfile = new VendorProfile
            {
                UserId = userId,

                FarmName = model.FarmName.Trim(),

                Address = model.Address.Trim(),

                City = model.City?.Trim() ?? string.Empty,

                Province = model.Province?.Trim() ?? string.Empty,

                FarmDescription =
                    model.FarmDescription?.Trim() ?? string.Empty,

              
                IsApproved = false,

                CreatedAt = DateTime.UtcNow
            };

            _context.VendorProfiles.Add(vendorProfile);

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Your vendor profile has been created successfully and is waiting for approval.";

            return RedirectToAction(nameof(Profile));
        }


        [HttpGet]
        public async Task<IActionResult> ProfileEdit()
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var vendorProfile = await _context.VendorProfiles
                .FirstOrDefaultAsync(v => v.UserId == userId);

            if (vendorProfile == null)
            {
                TempData["Error"] =
                    "Vendor profile could not be found.";

                return RedirectToAction(nameof(Dashboard));
            }

            return View(vendorProfile);
        }

[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> ProfileEdit(VendorProfile model)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            
            var vendorProfile = await _context.VendorProfiles
                .FirstOrDefaultAsync(v => v.UserId == userId);

            if (vendorProfile == null)
            {
                TempData["Error"] = "Vendor profile could not be found.";

                return RedirectToAction(nameof(Dashboard));
            }

        
            ModelState.Remove(nameof(VendorProfile.UserId));
            ModelState.Remove(nameof(VendorProfile.IsApproved));
            ModelState.Remove(nameof(VendorProfile.CreatedAt));
            ModelState.Remove(nameof(VendorProfile.User));
            ModelState.Remove(nameof(VendorProfile.VendorMarkets));
            ModelState.Remove(nameof(VendorProfile.Products));
            ModelState.Remove(nameof(VendorProfile.WeeklyStocks));
            ModelState.Remove(nameof(VendorProfile.PickupSlots));
            ModelState.Remove(nameof(VendorProfile.FavouriteVendors));

    
            if (string.IsNullOrWhiteSpace(model.FarmName))
            {
                ModelState.AddModelError(
                    nameof(VendorProfile.FarmName),
                    "Farm name is required.");
            }

            if (string.IsNullOrWhiteSpace(model.Address))
            {
                ModelState.AddModelError(
                    nameof(VendorProfile.Address),
                    "Address is required.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

          
            vendorProfile.FarmName =
                model.FarmName.Trim();

            vendorProfile.Address =
                model.Address.Trim();

            vendorProfile.City =
                model.City?.Trim() ?? string.Empty;

            vendorProfile.Province =
                model.Province?.Trim() ?? string.Empty;

            vendorProfile.FarmDescription =
                model.FarmDescription?.Trim() ?? string.Empty;

         

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Your profile has been updated successfully.";

            return RedirectToAction(nameof(Profile));
        }

        
[HttpGet]
public async Task<IActionResult> MyMarkets()
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var vendorProfile = await _context.VendorProfiles
                .FirstOrDefaultAsync(v => v.UserId == userId);

            if (vendorProfile == null)
            {
                TempData["Error"] = "Vendor profile could not be found.";
                return RedirectToAction(nameof(Dashboard));
            }

            var markets = await _context.VendorMarkets
                .Include(vm => vm.Market)
                .Where(vm => vm.VendorProfileId == vendorProfile.VendorProfileId)
                .OrderByDescending(vm => vm.IsActive)
                .ThenBy(vm => vm.Market!.MarketName)
                .ToListAsync();

            ViewBag.VendorProfile = vendorProfile;

            return View(markets);
        }

        [HttpGet]
        public async Task<IActionResult> Products()
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var vendorProfile = await _context.VendorProfiles
                .FirstOrDefaultAsync(v => v.UserId == userId);

            if (vendorProfile == null)
            {
                TempData["Error"] = "Vendor profile could not be found.";
                return RedirectToAction(nameof(Dashboard));
            }

            var products = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.VendorProfileId == vendorProfile.VendorProfileId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return View(products);
        }

        [HttpGet]
        public async Task<IActionResult> ProductCreate()
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var vendorProfile = await _context.VendorProfiles
                .FirstOrDefaultAsync(v => v.UserId == userId);

            if (vendorProfile == null)
            {
                TempData["Error"] = "Vendor profile could not be found.";
                return RedirectToAction(nameof(Dashboard));
            }

            await LoadProductCategories();

            return View(new Product());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProductCreate(Product model)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var vendorProfile = await _context.VendorProfiles
                .FirstOrDefaultAsync(v => v.UserId == userId);

            if (vendorProfile == null)
            {
                TempData["Error"] = "Vendor profile could not be found.";
                return RedirectToAction(nameof(Dashboard));
            }

            ModelState.Remove(nameof(Product.VendorProfile));
            ModelState.Remove(nameof(Product.Category));
            ModelState.Remove(nameof(Product.WeeklyStocks));
            ModelState.Remove(nameof(Product.OrderItems));
            ModelState.Remove(nameof(Product.FavouriteProducts));
            ModelState.Remove(nameof(Product.Reviews));
            ModelState.Remove(nameof(Product.CartItems));

            if (model.CategoryId <= 0)
            {
                ModelState.AddModelError(nameof(Product.CategoryId), "Please select a category.");
            }

            if (model.Price < 0)
            {
                ModelState.AddModelError(nameof(Product.Price), "Price cannot be negative.");
            }

            if (!ModelState.IsValid)
            {
                await LoadProductCategories();
                return View(model);
            }

            model.VendorProfileId = vendorProfile.VendorProfileId;
            model.ProductName = model.ProductName.Trim();
            model.Unit = model.Unit.Trim();
            model.Description = model.Description?.Trim() ?? string.Empty;
            model.IsAvailable = true;
            model.ModerationStatus = "Pending";
            model.CreatedAt = DateTime.UtcNow;

            _context.Products.Add(model);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Product added successfully and submitted for admin approval.";

            return RedirectToAction(nameof(Products));
        }

        [HttpGet]
        public async Task<IActionResult> ProductDetails(int id)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var vendorProfile = await _context.VendorProfiles
                .FirstOrDefaultAsync(v => v.UserId == userId);

            if (vendorProfile == null)
            {
                TempData["Error"] = "Vendor profile could not be found.";
                return RedirectToAction(nameof(Dashboard));
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.WeeklyStocks)
                .FirstOrDefaultAsync(p =>
                    p.ProductId == id &&
                    p.VendorProfileId == vendorProfile.VendorProfileId);

            if (product == null)
            {
                TempData["Error"] = "Product not found.";
                return RedirectToAction(nameof(Products));
            }

            return View(product);
        }

        [HttpGet]
        public async Task<IActionResult> ProductEdit(int id)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var vendorProfile = await _context.VendorProfiles
                .FirstOrDefaultAsync(v => v.UserId == userId);

            if (vendorProfile == null)
            {
                TempData["Error"] = "Vendor profile could not be found.";
                return RedirectToAction(nameof(Dashboard));
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(p =>
                    p.ProductId == id &&
                    p.VendorProfileId == vendorProfile.VendorProfileId);

            if (product == null)
            {
                TempData["Error"] = "Product not found.";
                return RedirectToAction(nameof(Products));
            }

            await LoadProductCategories();

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProductEdit(int id, Product model)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var vendorProfile = await _context.VendorProfiles
                .FirstOrDefaultAsync(v => v.UserId == userId);

            if (vendorProfile == null)
            {
                TempData["Error"] = "Vendor profile could not be found.";
                return RedirectToAction(nameof(Dashboard));
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(p =>
                    p.ProductId == id &&
                    p.VendorProfileId == vendorProfile.VendorProfileId);

            if (product == null)
            {
                TempData["Error"] = "Product not found.";
                return RedirectToAction(nameof(Products));
            }

            ModelState.Remove(nameof(Product.VendorProfile));
            ModelState.Remove(nameof(Product.Category));
            ModelState.Remove(nameof(Product.WeeklyStocks));
            ModelState.Remove(nameof(Product.OrderItems));
            ModelState.Remove(nameof(Product.FavouriteProducts));
            ModelState.Remove(nameof(Product.Reviews));
            ModelState.Remove(nameof(Product.CartItems));

            if (model.CategoryId <= 0)
            {
                ModelState.AddModelError(nameof(Product.CategoryId), "Please select a category.");
            }

            if (model.Price < 0)
            {
                ModelState.AddModelError(nameof(Product.Price), "Price cannot be negative.");
            }

            if (!ModelState.IsValid)
            {
                await LoadProductCategories();
                return View(model);
            }

            product.ProductName = model.ProductName.Trim();
            product.CategoryId = model.CategoryId;
            product.Description = model.Description?.Trim() ?? string.Empty;
            product.Unit = model.Unit.Trim();
            product.Price = model.Price;
            product.IsAvailable = model.IsAvailable;
            product.ModerationStatus = "Pending";

            await _context.SaveChangesAsync();

            TempData["Success"] = "Product updated successfully and sent for admin re-approval.";

            return RedirectToAction(nameof(Products));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProductDelete(int id)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var vendorProfile = await _context.VendorProfiles
                .FirstOrDefaultAsync(v => v.UserId == userId);

            if (vendorProfile == null)
            {
                TempData["Error"] = "Vendor profile could not be found.";
                return RedirectToAction(nameof(Dashboard));
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(p =>
                    p.ProductId == id &&
                    p.VendorProfileId == vendorProfile.VendorProfileId);

            if (product == null)
            {
                TempData["Error"] = "Product not found.";
                return RedirectToAction(nameof(Products));
            }

            var hasOrderItems = await _context.OrderItems
                .AnyAsync(i => i.ProductId == id);

            if (hasOrderItems)
            {
                TempData["Error"] = "This product cannot be deleted because it is already linked with an order.";
                return RedirectToAction(nameof(Products));
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Product deleted successfully.";

            return RedirectToAction(nameof(Products));
        }

        private async Task LoadProductCategories()
        {
            ViewBag.Categories = await _context.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.CategoryName)
                .ToListAsync();
        }

        [HttpGet]
        public async Task<IActionResult> WeeklyStock()
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var vendorProfile = await _context.VendorProfiles
                .FirstOrDefaultAsync(v => v.UserId == userId);

            if (vendorProfile == null)
            {
                TempData["Error"] = "Vendor profile could not be found.";
                return RedirectToAction(nameof(Dashboard));
            }

            var stocks = await _context.WeeklyStocks
                .Include(w => w.Product)
                .Where(w => w.VendorProfileId == vendorProfile.VendorProfileId)
                .OrderByDescending(w => w.WeekStartDate)
                .ThenBy(w => w.Product!.ProductName)
                .ToListAsync();

            return View(stocks);
        }

        [HttpGet]
        public async Task<IActionResult> WeeklyStockCreate()
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var vendorProfile = await _context.VendorProfiles
                .FirstOrDefaultAsync(v => v.UserId == userId);

            if (vendorProfile == null)
            {
                TempData["Error"] = "Vendor profile could not be found.";
                return RedirectToAction(nameof(Dashboard));
            }

            var products = await _context.Products
                .Include(p => p.Category)
                .Where(p =>
                    p.VendorProfileId == vendorProfile.VendorProfileId &&
                    p.IsAvailable &&
                    p.ModerationStatus == "Approved")
                .OrderBy(p => p.ProductName)
                .ToListAsync();

            if (!products.Any())
            {
                TempData["Error"] =
                    "You need at least one approved and available product before adding weekly stock.";

                return RedirectToAction(nameof(WeeklyStock));
            }

            ViewBag.Products = products;

            var model = new WeeklyStock
            {
                WeekStartDate = DateTime.Today,
                AvailableQuantity = 0,
                ReservedQuantity = 0
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> WeeklyStockCreate(WeeklyStock model)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var vendorProfile = await _context.VendorProfiles
                .FirstOrDefaultAsync(v => v.UserId == userId);

            if (vendorProfile == null)
            {
                TempData["Error"] = "Vendor profile could not be found.";
                return RedirectToAction(nameof(Dashboard));
            }

            ModelState.Remove("Product");
            ModelState.Remove("VendorProfile");

            if (model.ProductId <= 0)
            {
                ModelState.AddModelError(
                    "ProductId",
                    "Please select a product.");
            }

            if (model.AvailableQuantity < 0)
            {
                ModelState.AddModelError(
                    "AvailableQuantity",
                    "Available quantity cannot be negative.");
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(p =>
                    p.ProductId == model.ProductId &&
                    p.VendorProfileId == vendorProfile.VendorProfileId &&
                    p.IsAvailable &&
                    p.ModerationStatus == "Approved");

            if (product == null)
            {
                ModelState.AddModelError(
                    "ProductId",
                    "Selected product is not available for weekly stock.");
            }

            var weekStart = model.WeekStartDate.Date;

            var stockExists = await _context.WeeklyStocks
                .AnyAsync(w =>
                    w.ProductId == model.ProductId &&
                    w.VendorProfileId == vendorProfile.VendorProfileId &&
                    w.WeekStartDate == weekStart);

            if (stockExists)
            {
                ModelState.AddModelError(
                    "WeekStartDate",
                    "Weekly stock for this product and week already exists.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Products = await _context.Products
                    .Include(p => p.Category)
                    .Where(p =>
                        p.VendorProfileId == vendorProfile.VendorProfileId &&
                        p.IsAvailable &&
                        p.ModerationStatus == "Approved")
                    .OrderBy(p => p.ProductName)
                    .ToListAsync();

                return View(model);
            }

            model.VendorProfileId = vendorProfile.VendorProfileId;
            model.WeekStartDate = weekStart;
            model.ReservedQuantity = 0;
            model.UpdatedAt = DateTime.UtcNow;

            _context.WeeklyStocks.Add(model);

            await _context.SaveChangesAsync();

            TempData["Success"] = "Weekly stock added successfully.";

            return RedirectToAction(nameof(WeeklyStock));
        }

        [HttpGet]
        public async Task<IActionResult> WeeklyStockEdit(int id)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var vendorProfile = await _context.VendorProfiles
                .FirstOrDefaultAsync(v => v.UserId == userId);

            if (vendorProfile == null)
            {
                TempData["Error"] = "Vendor profile could not be found.";
                return RedirectToAction(nameof(Dashboard));
            }

            var stock = await _context.WeeklyStocks
                .Include(w => w.Product)
                .FirstOrDefaultAsync(w =>
                    w.WeeklyStockId == id &&
                    w.VendorProfileId == vendorProfile.VendorProfileId);

            if (stock == null)
            {
                TempData["Error"] = "Weekly stock not found.";
                return RedirectToAction(nameof(WeeklyStock));
            }

            ViewBag.Products = await _context.Products
                .Include(p => p.Category)
                .Where(p =>
                    p.VendorProfileId == vendorProfile.VendorProfileId &&
                    p.IsAvailable &&
                    p.ModerationStatus == "Approved")
                .OrderBy(p => p.ProductName)
                .ToListAsync();

            return View(stock);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> WeeklyStockEdit(
            int id,
            WeeklyStock model)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var vendorProfile = await _context.VendorProfiles
                .FirstOrDefaultAsync(v => v.UserId == userId);

            if (vendorProfile == null)
            {
                TempData["Error"] = "Vendor profile could not be found.";
                return RedirectToAction(nameof(Dashboard));
            }

            var stock = await _context.WeeklyStocks
                .FirstOrDefaultAsync(w =>
                    w.WeeklyStockId == id &&
                    w.VendorProfileId == vendorProfile.VendorProfileId);

            if (stock == null)
            {
                TempData["Error"] = "Weekly stock not found.";
                return RedirectToAction(nameof(WeeklyStock));
            }

            ModelState.Remove("Product");
            ModelState.Remove("VendorProfile");
            ModelState.Remove("ReservedQuantity");

            if (model.ProductId <= 0)
            {
                ModelState.AddModelError(
                    "ProductId",
                    "Please select a product.");
            }

            if (model.AvailableQuantity < 0)
            {
                ModelState.AddModelError(
                    "AvailableQuantity",
                    "Available quantity cannot be negative.");
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(p =>
                    p.ProductId == model.ProductId &&
                    p.VendorProfileId == vendorProfile.VendorProfileId &&
                    p.IsAvailable &&
                    p.ModerationStatus == "Approved");

            if (product == null)
            {
                ModelState.AddModelError(
                    "ProductId",
                    "Selected product is not available.");
            }

            var weekStart = model.WeekStartDate.Date;

            var duplicateExists = await _context.WeeklyStocks
                .AnyAsync(w =>
                    w.WeeklyStockId != id &&
                    w.ProductId == model.ProductId &&
                    w.VendorProfileId == vendorProfile.VendorProfileId &&
                    w.WeekStartDate == weekStart);

            if (duplicateExists)
            {
                ModelState.AddModelError(
                    "WeekStartDate",
                    "Weekly stock for this product and week already exists.");
            }

            if (model.AvailableQuantity < stock.ReservedQuantity)
            {
                ModelState.AddModelError(
                    "AvailableQuantity",
                    "Available quantity cannot be less than the reserved quantity.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Products = await _context.Products
                    .Include(p => p.Category)
                    .Where(p =>
                        p.VendorProfileId == vendorProfile.VendorProfileId &&
                        p.IsAvailable &&
                        p.ModerationStatus == "Approved")
                    .OrderBy(p => p.ProductName)
                    .ToListAsync();

                return View(model);
            }

            stock.ProductId = model.ProductId;
            stock.WeekStartDate = weekStart;
            stock.AvailableQuantity = model.AvailableQuantity;
            stock.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Weekly stock updated successfully.";

            return RedirectToAction(nameof(WeeklyStock));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> WeeklyStockDelete(int id)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var vendorProfile = await _context.VendorProfiles
                .FirstOrDefaultAsync(v => v.UserId == userId);

            if (vendorProfile == null)
            {
                TempData["Error"] = "Vendor profile could not be found.";
                return RedirectToAction(nameof(Dashboard));
            }

            var stock = await _context.WeeklyStocks
                .FirstOrDefaultAsync(w =>
                    w.WeeklyStockId == id &&
                    w.VendorProfileId == vendorProfile.VendorProfileId);

            if (stock == null)
            {
                TempData["Error"] = "Weekly stock not found.";
                return RedirectToAction(nameof(WeeklyStock));
            }

            if (stock.ReservedQuantity > 0)
            {
                TempData["Error"] =
                    "This weekly stock cannot be deleted because quantity has already been reserved.";

                return RedirectToAction(nameof(WeeklyStock));
            }

            _context.WeeklyStocks.Remove(stock);

            await _context.SaveChangesAsync();

            TempData["Success"] = "Weekly stock deleted successfully.";

            return RedirectToAction(nameof(WeeklyStock));
        }

        [HttpGet]
        public async Task<IActionResult> PickupSlots()
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var vendorProfile = await _context.VendorProfiles
                .FirstOrDefaultAsync(v => v.UserId == userId);

            if (vendorProfile == null)
            {
                TempData["Error"] = "Vendor profile could not be found.";
                return RedirectToAction(nameof(Dashboard));
            }

            var slots = await _context.PickupSlots
                .Include(p => p.Orders)
                .Where(p => p.VendorProfileId == vendorProfile.VendorProfileId)
                .OrderByDescending(p => p.PickupDate)
                .ThenBy(p => p.StartTime)
                .ToListAsync();

            return View(slots);
        }

        [HttpGet]
        public async Task<IActionResult> PickupSlotCreate()
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var vendorProfile = await _context.VendorProfiles
                .FirstOrDefaultAsync(v => v.UserId == userId);

            if (vendorProfile == null)
            {
                TempData["Error"] = "Vendor profile could not be found.";
                return RedirectToAction(nameof(Dashboard));
            }

            return View(new PickupSlot
            {
                PickupDate = DateTime.Today,
                StartTime = new TimeSpan(10, 0, 0),
                EndTime = new TimeSpan(12, 0, 0),
                MaxOrders = 10,
                IsAvailable = true
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PickupSlotCreate(PickupSlot model)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var vendorProfile = await _context.VendorProfiles
                .FirstOrDefaultAsync(v => v.UserId == userId);

            if (vendorProfile == null)
            {
                TempData["Error"] = "Vendor profile could not be found.";
                return RedirectToAction(nameof(Dashboard));
            }

            ModelState.Remove("VendorProfile");
            ModelState.Remove("Orders");

            if (model.PickupDate.Date < DateTime.Today)
            {
                ModelState.AddModelError(
                    "PickupDate",
                    "Pickup date cannot be in the past.");
            }

            if (model.StartTime >= model.EndTime)
            {
                ModelState.AddModelError(
                    "EndTime",
                    "End time must be later than start time.");
            }

            if (model.MaxOrders <= 0)
            {
                ModelState.AddModelError(
                    "MaxOrders",
                    "Maximum orders must be greater than zero.");
            }

            var pickupDate = model.PickupDate.Date;

            var slotExists = await _context.PickupSlots
                .AnyAsync(p =>
                    p.VendorProfileId == vendorProfile.VendorProfileId &&
                    p.PickupDate.Date == pickupDate &&
                    p.StartTime == model.StartTime &&
                    p.EndTime == model.EndTime);

            if (slotExists)
            {
                ModelState.AddModelError(
                    "PickupDate",
                    "A pickup slot with the same date and time already exists.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            model.VendorProfileId = vendorProfile.VendorProfileId;
            model.PickupDate = pickupDate;
            model.IsAvailable = true;

            _context.PickupSlots.Add(model);

            await _context.SaveChangesAsync();

            TempData["Success"] = "Pickup slot added successfully.";

            return RedirectToAction(nameof(PickupSlots));
        }

        [HttpGet]
        public async Task<IActionResult> PickupSlotEdit(int id)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var vendorProfile = await _context.VendorProfiles
                .FirstOrDefaultAsync(v => v.UserId == userId);

            if (vendorProfile == null)
            {
                TempData["Error"] = "Vendor profile could not be found.";
                return RedirectToAction(nameof(Dashboard));
            }

            var slot = await _context.PickupSlots
                .Include(p => p.Orders)
                .FirstOrDefaultAsync(p =>
                    p.PickupSlotId == id &&
                    p.VendorProfileId == vendorProfile.VendorProfileId);

            if (slot == null)
            {
                TempData["Error"] = "Pickup slot not found.";
                return RedirectToAction(nameof(PickupSlots));
            }

            return View(slot);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PickupSlotEdit(
            int id,
            PickupSlot model)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var vendorProfile = await _context.VendorProfiles
                .FirstOrDefaultAsync(v => v.UserId == userId);

            if (vendorProfile == null)
            {
                TempData["Error"] = "Vendor profile could not be found.";
                return RedirectToAction(nameof(Dashboard));
            }

            var slot = await _context.PickupSlots
                .Include(p => p.Orders)
                .FirstOrDefaultAsync(p =>
                    p.PickupSlotId == id &&
                    p.VendorProfileId == vendorProfile.VendorProfileId);

            if (slot == null)
            {
                TempData["Error"] = "Pickup slot not found.";
                return RedirectToAction(nameof(PickupSlots));
            }

            ModelState.Remove("VendorProfile");
            ModelState.Remove("Orders");

            if (model.PickupDate.Date < DateTime.Today)
            {
                ModelState.AddModelError(
                    "PickupDate",
                    "Pickup date cannot be in the past.");
            }

            if (model.StartTime >= model.EndTime)
            {
                ModelState.AddModelError(
                    "EndTime",
                    "End time must be later than start time.");
            }

            if (model.MaxOrders <= 0)
            {
                ModelState.AddModelError(
                    "MaxOrders",
                    "Maximum orders must be greater than zero.");
            }

            var bookedOrders = slot.Orders.Count;

            if (model.MaxOrders < bookedOrders)
            {
                ModelState.AddModelError(
                    "MaxOrders",
                    $"Maximum orders cannot be less than the current booked orders ({bookedOrders}).");
            }

            var pickupDate = model.PickupDate.Date;

            var slotExists = await _context.PickupSlots
                .AnyAsync(p =>
                    p.PickupSlotId != id &&
                    p.VendorProfileId == vendorProfile.VendorProfileId &&
                    p.PickupDate.Date == pickupDate &&
                    p.StartTime == model.StartTime &&
                    p.EndTime == model.EndTime);

            if (slotExists)
            {
                ModelState.AddModelError(
                    "PickupDate",
                    "A pickup slot with the same date and time already exists.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            slot.PickupDate = pickupDate;
            slot.StartTime = model.StartTime;
            slot.EndTime = model.EndTime;
            slot.MaxOrders = model.MaxOrders;
            slot.IsAvailable = model.IsAvailable;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Pickup slot updated successfully.";

            return RedirectToAction(nameof(PickupSlots));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PickupSlotDelete(int id)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var vendorProfile = await _context.VendorProfiles
                .FirstOrDefaultAsync(v => v.UserId == userId);

            if (vendorProfile == null)
            {
                TempData["Error"] = "Vendor profile could not be found.";
                return RedirectToAction(nameof(Dashboard));
            }

            var slot = await _context.PickupSlots
                .Include(p => p.Orders)
                .FirstOrDefaultAsync(p =>
                    p.PickupSlotId == id &&
                    p.VendorProfileId == vendorProfile.VendorProfileId);

            if (slot == null)
            {
                TempData["Error"] = "Pickup slot not found.";
                return RedirectToAction(nameof(PickupSlots));
            }

            if (slot.Orders.Any())
            {
                TempData["Error"] =
                    "This pickup slot cannot be deleted because it already has orders.";

                return RedirectToAction(nameof(PickupSlots));
            }

            _context.PickupSlots.Remove(slot);

            await _context.SaveChangesAsync();

            TempData["Success"] = "Pickup slot deleted successfully.";

            return RedirectToAction(nameof(PickupSlots));
        }

        public async Task<IActionResult> IncomingOrders()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Account");

            var vendor = await _context.VendorProfiles
                .FirstOrDefaultAsync(v => v.UserId == user.Id);

            if (vendor == null)
                return NotFound();

            var orders = await _context.Orders
                .Include(o => o.CustomerProfile)
                .Include(o => o.PickupSlot)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Where(o => o.OrderItems.Any(oi =>
                    oi.Product != null &&
                    oi.Product.VendorProfileId == vendor.VendorProfileId))
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        public async Task<IActionResult> OrderDetails(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Account");

            var vendor = await _context.VendorProfiles
                .FirstOrDefaultAsync(v => v.UserId == user.Id);

            if (vendor == null)
                return NotFound();

            var order = await _context.Orders
                .Include(o => o.CustomerProfile)
                .Include(o => o.PickupSlot)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o =>
                    o.OrderId == id &&
                    o.OrderItems.Any(oi =>
                        oi.Product != null &&
                        oi.Product.VendorProfileId == vendor.VendorProfileId));

            if (order == null)
                return NotFound();

            return View(order);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateOrderStatus(int id, string status)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Account");

            var vendor = await _context.VendorProfiles
                .FirstOrDefaultAsync(v => v.UserId == user.Id);

            if (vendor == null)
                return NotFound();

            var allowedStatuses = new[]
            {
        "VendorApproved",
        "VendorRejected"
    };

            if (!allowedStatuses.Contains(status))
                return BadRequest();

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o =>
                    o.OrderId == id &&
                    o.Status == "Pending" &&
                    o.OrderItems.Any(oi =>
                        oi.Product != null &&
                        oi.Product.VendorProfileId == vendor.VendorProfileId));

            if (order == null)
            {
                TempData["ErrorMessage"] =
                    "Order was not found, is no longer pending, or does not belong to your products.";

                return RedirectToAction(nameof(IncomingOrders));
            }

            order.Status = status;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = status == "VendorApproved"
                ? $"Order #{order.OrderNumber} has been approved and sent to admin for final approval."
                : $"Order #{order.OrderNumber} has been rejected.";

            return RedirectToAction(nameof(IncomingOrders));
        }

        [Authorize(Roles = "Vendor")]
        public async Task<IActionResult> Orders()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Account");

            var vendor = await _context.VendorProfiles
                .FirstOrDefaultAsync(v => v.UserId == user.Id);

            if (vendor == null)
                return NotFound();

            var orders = await _context.Orders
                .Include(o => o.CustomerProfile)
                .Include(o => o.PickupSlot)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Where(o => o.OrderItems.Any(oi =>
                    oi.Product != null &&
                    oi.Product.VendorProfileId == vendor.VendorProfileId))
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View("IncomingOrders", orders);
        }

        public async Task<IActionResult> CustomerReviews()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Account");

            var vendor = await _context.VendorProfiles
                .FirstOrDefaultAsync(v => v.UserId == user.Id);

            if (vendor == null)
                return NotFound();

            var reviews = await _context.Reviews
                .Include(r => r.CustomerProfile)
                .Include(r => r.Product)
                .Include(r => r.ReviewResponses)
                    .ThenInclude(rr => rr.User)
                .Where(r =>
                    r.Product != null &&
                    r.Product.VendorProfileId == vendor.VendorProfileId &&
                    r.IsApproved)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return View("CustomerReviews", reviews);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddReviewResponse(
            int reviewId,
            string response)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Account");

            if (string.IsNullOrWhiteSpace(response))
            {
                TempData["Error"] = "Please enter a response.";
                return RedirectToAction(nameof(CustomerReviews));
            }

            if (response.Length > 1000)
            {
                TempData["Error"] = "Response cannot exceed 1000 characters.";
                return RedirectToAction(nameof(CustomerReviews));
            }

            var vendor = await _context.VendorProfiles
                .FirstOrDefaultAsync(v => v.UserId == user.Id);

            if (vendor == null)
                return NotFound();

            var review = await _context.Reviews
                .Include(r => r.Product)
                .Include(r => r.ReviewResponses)
                .FirstOrDefaultAsync(r =>
                    r.ReviewId == reviewId &&
                    r.IsApproved &&
                    r.Product != null &&
                    r.Product.VendorProfileId == vendor.VendorProfileId);

            if (review == null)
                return NotFound();

            var existingResponse = review.ReviewResponses
                .FirstOrDefault(r => r.UserId == user.Id);

            if (existingResponse != null)
            {
                existingResponse.Response = response.Trim();
                existingResponse.CreatedAt = DateTime.UtcNow;
            }
            else
            {
                var reviewResponse = new ReviewResponse
                {
                    ReviewId = review.ReviewId,
                    UserId = user.Id,
                    Response = response.Trim(),
                    CreatedAt = DateTime.UtcNow
                };

                _context.ReviewResponses.Add(reviewResponse);
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Your response has been added successfully.";

            return RedirectToAction(nameof(CustomerReviews));
        }
    }
}