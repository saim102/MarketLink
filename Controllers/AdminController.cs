using IdentityDemoApp.Data;
using IdentityDemoApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IdentityDemoApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }


        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            ViewBag.TotalVendors = await _context.VendorProfiles.CountAsync();

            ViewBag.ApprovedVendors = await _context.VendorProfiles
                .CountAsync(v => v.IsApproved);

            ViewBag.PendingVendors = await _context.VendorProfiles
                .CountAsync(v => !v.IsApproved);

            ViewBag.TotalCustomers = await _context.CustomerProfiles.CountAsync();

            ViewBag.PendingCustomers = await _context.CustomerProfiles
                .CountAsync(c => !c.IsApproved);

            ViewBag.TotalProducts = await _context.Products.CountAsync();

            ViewBag.PendingProducts = await _context.Products
                .CountAsync(p => p.ModerationStatus == "Pending");

            ViewBag.TotalOrders = await _context.Orders.CountAsync();

            ViewBag.TotalMarkets = await _context.Markets.CountAsync();

            ViewBag.TotalCategories = await _context.Categories.CountAsync();

            ViewBag.PendingReviews = await _context.Reviews
                .CountAsync(r => !r.IsApproved);

            ViewBag.RecentVendors = await _context.VendorProfiles
                .Include(v => v.User)
                .OrderByDescending(v => v.CreatedAt)
                .Take(5)
                .ToListAsync();

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Vendors()
        {
            var vendors = await _context.VendorProfiles
                .Include(v => v.User)
                .OrderByDescending(v => v.CreatedAt)
                .ToListAsync();

            return View(vendors);
        }

       

        [HttpGet]
        public async Task<IActionResult> VendorDetails(int id)
        {
            var vendor = await _context.VendorProfiles
                .Include(v => v.User)
                .FirstOrDefaultAsync(
                    v => v.VendorProfileId == id);

            if (vendor == null)
            {
                return NotFound();
            }

            return View(vendor);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveVendor(int id)
        {
            var vendor =
                await _context.VendorProfiles
                    .FirstOrDefaultAsync(
                        v => v.VendorProfileId == id);

            if (vendor == null)
            {
                TempData["Error"] = "Vendor not found.";

                return RedirectToAction(nameof(Vendors));
            }

            vendor.IsApproved = true;

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Vendor approved successfully.";

            return RedirectToAction(nameof(Vendors));
        }


       

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectVendor(int id)
        {
            var vendor =
                await _context.VendorProfiles
                    .FirstOrDefaultAsync(
                        v => v.VendorProfileId == id);

            if (vendor == null)
            {
                TempData["Error"] =
                    "Vendor not found.";

                return RedirectToAction(
                    nameof(Vendors));
            }

            vendor.IsApproved = false;

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Vendor approval rejected.";

            return RedirectToAction(
                nameof(Vendors));
        }

       
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> DeleteVendor(int id)
        {
            var vendor =
                await _context.VendorProfiles
                    .FirstOrDefaultAsync(
                        v => v.VendorProfileId == id);

            if (vendor == null)
            {
                TempData["Error"] =
                    "Vendor not found.";

                return RedirectToAction(
                    nameof(Vendors));
            }

            _context.VendorProfiles.Remove(vendor);

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Vendor deleted successfully.";

            return RedirectToAction(
                nameof(Vendors));
        }

       

        [HttpGet]
        public async Task<IActionResult> ManageCustomers(
            string? search)
        {
            var query = _context.CustomerProfiles
                .Include(c => c.User)
                .AsQueryable();

       
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(c =>
                    c.FullName.Contains(search) ||
                    c.ContactNumber.Contains(search) ||
                    c.City.Contains(search) ||
                    (c.User != null &&
                     c.User.Email != null &&
                     c.User.Email.Contains(search)));
            }

            var customers = await query
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            ViewBag.Search = search;

            return View(customers);
        }


        

        [HttpGet]
        public async Task<IActionResult> CustomerDetails(
            int id)
        {
            var customer = await _context.CustomerProfiles
                .Include(c => c.User)
                .Include(c => c.Orders)
                .Include(c => c.Reviews)
                .Include(c => c.FavouriteProducts)
                .Include(c => c.FavouriteVendors)
                .FirstOrDefaultAsync(
                    c => c.CustomerProfileId == id);

            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
        }

        

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveCustomer(int id)
        {
            var customer =
                await _context.CustomerProfiles
                    .FirstOrDefaultAsync(
                        c => c.CustomerProfileId == id);

            if (customer == null)
            {
                TempData["Error"] =
                    "Customer not found.";

                return RedirectToAction(
                    nameof(ManageCustomers));
            }

            customer.IsApproved = true;

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Customer approved successfully.";

            return RedirectToAction(
                nameof(ManageCustomers));
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectCustomer(int id)
        {
            var customer =
                await _context.CustomerProfiles
                    .FirstOrDefaultAsync(
                        c => c.CustomerProfileId == id);

            if (customer == null)
            {
                TempData["Error"] =
                    "Customer not found.";

                return RedirectToAction(
                    nameof(ManageCustomers));
            }

            customer.IsApproved = false;

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Customer approval rejected.";

            return RedirectToAction(
                nameof(ManageCustomers));
        }


        [HttpGet]
        public async Task<IActionResult> Markets(string search = "", string status = "All")
        {
            var query = _context.Markets
                .Include(m => m.VendorMarkets)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(m =>
                    m.MarketName.Contains(search) ||
                    m.Location.Contains(search) ||
                    m.City.Contains(search));
            }

            if (status == "Active")
            {
                query = query.Where(m => m.IsActive);
            }
            else if (status == "Inactive")
            {
                query = query.Where(m => !m.IsActive);
            }

            var markets = await query
                .OrderBy(m => m.MarketName)
                .ToListAsync();

            ViewBag.Search = search;
            ViewBag.Status = status;

            ViewBag.TotalMarkets = await _context.Markets.CountAsync();
            ViewBag.ActiveMarkets = await _context.Markets.CountAsync(m => m.IsActive);
            ViewBag.InactiveMarkets = await _context.Markets.CountAsync(m => !m.IsActive);

            return View(markets);
        }


        [HttpGet]
        public IActionResult MarketCreate()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarketCreate(Market market)
        {
            if (market.OpeningTime.HasValue &&
                market.ClosingTime.HasValue &&
                market.OpeningTime >= market.ClosingTime)
            {
                ModelState.AddModelError(
                    "ClosingTime",
                    "Closing time must be later than opening time.");
            }

            if (!ModelState.IsValid)
            {
                return View(market);
            }

            try
            {
                market.MarketName =
                    market.MarketName.Trim();

                market.Location =
                    market.Location.Trim();

                market.City =
                    market.City?.Trim() ?? string.Empty;

                market.OperatingDays =
                    market.OperatingDays?.Trim() ?? string.Empty;

                market.MapLink =
                    market.MapLink?.Trim();

                market.Description =
                    market.Description?.Trim() ?? string.Empty;

                _context.Markets.Add(market);

                await _context.SaveChangesAsync();

                TempData["Success"] =
                    "Market added successfully.";

                return RedirectToAction(
                    nameof(Markets));
            }
            catch
            {
                TempData["Error"] =
                    "Unable to add market. Please try again.";

                return View(market);
            }
        }

        [HttpGet]
        public async Task<IActionResult> MarketEdit(int id)
        {
            var market = await _context.Markets.FindAsync(id);

            if (market == null)
            {
                return NotFound();
            }

            return View(market);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarketEdit(
    int id,
    Market market)
        {
            if (id != market.MarketId)
            {
                return NotFound();
            }

            if (market.OpeningTime.HasValue &&
                market.ClosingTime.HasValue &&
                market.OpeningTime >= market.ClosingTime)
            {
                ModelState.AddModelError(
                    "ClosingTime",
                    "Closing time must be later than opening time.");
            }

            if (!ModelState.IsValid)
            {
                return View(market);
            }

            try
            {
                var existingMarket =
                    await _context.Markets
                        .FirstOrDefaultAsync(m =>
                            m.MarketId == id);

                if (existingMarket == null)
                {
                    return NotFound();
                }

                existingMarket.MarketName =
                    market.MarketName.Trim();

                existingMarket.Location =
                    market.Location.Trim();

                existingMarket.City =
                    market.City?.Trim() ?? string.Empty;

                existingMarket.OperatingDays =
                    market.OperatingDays?.Trim() ?? string.Empty;

                existingMarket.OpeningTime =
                    market.OpeningTime;

                existingMarket.ClosingTime =
                    market.ClosingTime;

                existingMarket.Latitude =
                    market.Latitude;

                existingMarket.Longitude =
                    market.Longitude;

                existingMarket.MapLink =
                    market.MapLink?.Trim();

                existingMarket.Description =
                    market.Description?.Trim() ?? string.Empty;

                existingMarket.IsActive =
                    market.IsActive;

                await _context.SaveChangesAsync();

                TempData["Success"] =
                    "Market updated successfully.";

                return RedirectToAction(
                    nameof(Markets));
            }
            catch
            {
                TempData["Error"] =
                    "Unable to update market. Please try again.";

                return View(market);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarketDelete(int id)
        {
            try
            {
                var market = await _context.Markets
                    .Include(m => m.VendorMarkets)
                    .FirstOrDefaultAsync(m => m.MarketId == id);

                if (market == null)
                {
                    TempData["Error"] = "Market not found.";
                    return RedirectToAction(nameof(Markets));
                }

           
                if (market.VendorMarkets != null && market.VendorMarkets.Any())
                {
                    TempData["Error"] =
                        "This market cannot be deleted because vendors are linked to it. Deactivate it instead.";

                    return RedirectToAction(nameof(Markets));
                }

                _context.Markets.Remove(market);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Market deleted successfully.";
            }
            catch
            {
                TempData["Error"] = "Unable to delete market. Please try again.";
            }

            return RedirectToAction(nameof(Markets));
        }


       

        [HttpGet]
        public async Task<IActionResult> VendorMarkets()
        {
            var assignments = await _context.VendorMarkets
                .Include(vm => vm.VendorProfile)
                .Include(vm => vm.Market)
                .OrderBy(vm => vm.Market!.MarketName)
                .ThenBy(vm => vm.VendorProfile!.FarmName)
                .ToListAsync();

            ViewBag.TotalAssignments = assignments.Count;
            ViewBag.ActiveAssignments = assignments.Count(x => x.IsActive);
            ViewBag.InactiveAssignments = assignments.Count(x => !x.IsActive);

            return View(assignments);
        }


        [HttpGet]
        public async Task<IActionResult> AssignVendorMarket()
        {
            var vendors = await _context.VendorProfiles
                .Where(v => v.IsApproved)
                .OrderBy(v => v.FarmName)
                .ToListAsync();

            var markets = await _context.Markets
                .Where(m => m.IsActive)
                .OrderBy(m => m.MarketName)
                .ToListAsync();

            ViewBag.Vendors = vendors;
            ViewBag.Markets = markets;

            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignVendorMarket(
            int VendorProfileId,
            int MarketId)
        {
            if (VendorProfileId <= 0 || MarketId <= 0)
            {
                TempData["Error"] = "Please select a vendor and market.";
                return RedirectToAction(nameof(AssignVendorMarket));
            }

            try
            {
                
                var vendor = await _context.VendorProfiles
                    .FirstOrDefaultAsync(v =>
                        v.VendorProfileId == VendorProfileId &&
                        v.IsApproved);

                if (vendor == null)
                {
                    TempData["Error"] = "Vendor not found or not approved.";
                    return RedirectToAction(nameof(AssignVendorMarket));
                }

             
                var market = await _context.Markets
                    .FirstOrDefaultAsync(m =>
                        m.MarketId == MarketId &&
                        m.IsActive);

                if (market == null)
                {
                    TempData["Error"] = "Market not found or inactive.";
                    return RedirectToAction(nameof(AssignVendorMarket));
                }

               
                var existingAssignment = await _context.VendorMarkets
                    .FirstOrDefaultAsync(vm =>
                        vm.VendorProfileId == VendorProfileId &&
                        vm.MarketId == MarketId);

                if (existingAssignment != null)
                {
                    if (!existingAssignment.IsActive)
                    {
                        existingAssignment.IsActive = true;
                        await _context.SaveChangesAsync();

                        TempData["Success"] =
                            "Vendor has been assigned to the market again.";
                    }
                    else
                    {
                        TempData["Error"] =
                            "This vendor is already assigned to this market.";
                    }

                    return RedirectToAction(nameof(VendorMarkets));
                }

               
                var vendorMarket = new VendorMarket
                {
                    VendorProfileId = VendorProfileId,
                    MarketId = MarketId,
                    IsActive = true
                };

                _context.VendorMarkets.Add(vendorMarket);

                await _context.SaveChangesAsync();

                TempData["Success"] =
                    "Vendor assigned to market successfully.";

                return RedirectToAction(nameof(VendorMarkets));
            }
            catch
            {
                TempData["Error"] =
                    "Unable to assign vendor to market. Please try again.";

                return RedirectToAction(nameof(AssignVendorMarket));
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveVendorMarket(int id)
        {
            try
            {
                var assignment = await _context.VendorMarkets
                    .FirstOrDefaultAsync(vm => vm.VendorMarketId == id);

                if (assignment == null)
                {
                    TempData["Error"] = "Assignment not found.";
                    return RedirectToAction(nameof(VendorMarkets));
                }

                _context.VendorMarkets.Remove(assignment);

                await _context.SaveChangesAsync();

                TempData["Success"] =
                    "Vendor market assignment removed successfully.";
            }
            catch
            {
                TempData["Error"] =
                    "Unable to remove vendor market assignment.";
            }

            return RedirectToAction(nameof(VendorMarkets));
        }

       
        [HttpGet]
        public async Task<IActionResult> Categories(
            string search = "",
            string status = "All")
        {
            var query = _context.Categories
                .Include(c => c.Products)
                .AsQueryable();

      
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(c =>
                    c.CategoryName.Contains(search) ||
                    c.Description.Contains(search));
            }

        
            if (status == "Active")
            {
                query = query.Where(c => c.IsActive);
            }
            else if (status == "Inactive")
            {
                query = query.Where(c => !c.IsActive);
            }

            var categories = await query
                .OrderBy(c => c.CategoryName)
                .ToListAsync();

            ViewBag.Search = search;
            ViewBag.Status = status;

            ViewBag.TotalCategories =
                await _context.Categories.CountAsync();

            ViewBag.ActiveCategories =
                await _context.Categories.CountAsync(c => c.IsActive);

            ViewBag.InactiveCategories =
                await _context.Categories.CountAsync(c => !c.IsActive);

            return View(categories);
        }


       

        [HttpGet]
        public IActionResult CategoryCreate()
        {
            return View();
        }


       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CategoryCreate(Category category)
        {
            if (!ModelState.IsValid)
            {
                return View(category);
            }

            try
            {
                category.CategoryName =
                    category.CategoryName.Trim();

                category.Description =
                    category.Description?.Trim() ?? string.Empty;

              
                bool exists = await _context.Categories
                    .AnyAsync(c =>
                        c.CategoryName.ToLower() ==
                        category.CategoryName.ToLower());

                if (exists)
                {
                    ModelState.AddModelError(
                        "CategoryName",
                        "This category already exists.");

                    return View(category);
                }

                _context.Categories.Add(category);

                await _context.SaveChangesAsync();

                TempData["Success"] =
                    "Category added successfully.";

                return RedirectToAction(nameof(Categories));
            }
            catch
            {
                TempData["Error"] =
                    "Unable to add category. Please try again.";

                return View(category);
            }
        }


       

        [HttpGet]
        public async Task<IActionResult> CategoryEdit(int id)
        {
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CategoryEdit(
            int id,
            Category category)
        {
            if (id != category.CategoryId)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(category);
            }

            try
            {
                var existingCategory =
                    await _context.Categories
                        .FirstOrDefaultAsync(c =>
                            c.CategoryId == id);

                if (existingCategory == null)
                {
                    return NotFound();
                }

                category.CategoryName =
                    category.CategoryName.Trim();

                category.Description =
                    category.Description?.Trim() ?? string.Empty;

            
                bool duplicate = await _context.Categories
                    .AnyAsync(c =>
                        c.CategoryId != id &&
                        c.CategoryName.ToLower() ==
                        category.CategoryName.ToLower());

                if (duplicate)
                {
                    ModelState.AddModelError(
                        "CategoryName",
                        "Another category with this name already exists.");

                    return View(category);
                }

                existingCategory.CategoryName =
                    category.CategoryName;

                existingCategory.Description =
                    category.Description;

                existingCategory.IsActive =
                    category.IsActive;

                await _context.SaveChangesAsync();

                TempData["Success"] =
                    "Category updated successfully.";

                return RedirectToAction(nameof(Categories));
            }
            catch
            {
                TempData["Error"] =
                    "Unable to update category. Please try again.";

                return View(category);
            }
        }


       

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CategoryDelete(int id)
        {
            try
            {
                var category = await _context.Categories
                    .Include(c => c.Products)
                    .FirstOrDefaultAsync(c =>
                        c.CategoryId == id);

                if (category == null)
                {
                    TempData["Error"] =
                        "Category not found.";

                    return RedirectToAction(nameof(Categories));
                }

                
                if (category.Products != null &&
                    category.Products.Any())
                {
                    TempData["Error"] =
                        "This category cannot be deleted because products are linked to it. Deactivate it instead.";

                    return RedirectToAction(nameof(Categories));
                }

                _context.Categories.Remove(category);

                await _context.SaveChangesAsync();

                TempData["Success"] =
                    "Category deleted successfully.";
            }
            catch
            {
                TempData["Error"] =
                    "Unable to delete category. Please try again.";
            }

            return RedirectToAction(nameof(Categories));
        }

       
        [HttpGet]
        public async Task<IActionResult> ProductModeration(
            string search = "",
            string status = "All",
            int categoryId = 0)
        {
            var query = _context.Products
                .Include(p => p.VendorProfile)
                .Include(p => p.Category)
                .AsQueryable();

        
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(p =>
                    p.ProductName.Contains(search) ||
                    p.Unit.Contains(search) ||
                    (p.VendorProfile != null &&
                     p.VendorProfile.FarmName.Contains(search)) ||
                    (p.Category != null &&
                     p.Category.CategoryName.Contains(search)));
            }

          
            if (status == "Pending")
            {
                query = query.Where(p =>
                    p.ModerationStatus == "Pending");
            }
            else if (status == "Approved")
            {
                query = query.Where(p =>
                    p.ModerationStatus == "Approved");
            }
            else if (status == "Rejected")
            {
                query = query.Where(p =>
                    p.ModerationStatus == "Rejected");
            }

          
            if (categoryId > 0)
            {
                query = query.Where(p =>
                    p.CategoryId == categoryId);
            }

            var products = await query
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            ViewBag.Search = search;
            ViewBag.Status = status;
            ViewBag.CategoryId = categoryId;

         
            ViewBag.Categories = await _context.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.CategoryName)
                .ToListAsync();

          
            ViewBag.TotalProducts =
                await _context.Products.CountAsync();

            ViewBag.PendingProducts =
                await _context.Products.CountAsync(p =>
                    p.ModerationStatus == "Pending");

            ViewBag.ApprovedProducts =
                await _context.Products.CountAsync(p =>
                    p.ModerationStatus == "Approved");

            ViewBag.RejectedProducts =
                await _context.Products.CountAsync(p =>
                    p.ModerationStatus == "Rejected");

            return View(products);
        }

        [HttpGet]
        public async Task<IActionResult> ProductModerationDetails(int id)
        {
            var product = await _context.Products
                .Include(p => p.VendorProfile)
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p =>
                    p.ProductId == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveProduct(int id)
        {
            try
            {
                var product = await _context.Products
                    .FirstOrDefaultAsync(p =>
                        p.ProductId == id);

                if (product == null)
                {
                    TempData["Error"] =
                        "Product not found.";

                    return RedirectToAction(
                        nameof(ProductModeration));
                }

                product.ModerationStatus = "Approved";

                await _context.SaveChangesAsync();

                TempData["Success"] =
                    "Product approved successfully.";
            }
            catch
            {
                TempData["Error"] =
                    "Unable to approve product. Please try again.";
            }

            return RedirectToAction(
                nameof(ProductModeration));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectProduct(int id)
        {
            try
            {
                var product = await _context.Products
                    .FirstOrDefaultAsync(p =>
                        p.ProductId == id);

                if (product == null)
                {
                    TempData["Error"] =
                        "Product not found.";

                    return RedirectToAction(
                        nameof(ProductModeration));
                }

                product.ModerationStatus = "Rejected";

                await _context.SaveChangesAsync();

                TempData["Success"] =
                    "Product rejected successfully.";
            }
            catch
            {
                TempData["Error"] =
                    "Unable to reject product. Please try again.";
            }

            return RedirectToAction(
                nameof(ProductModeration));
        }

      
        [HttpGet]
        public async Task<IActionResult> ReviewModeration(
            string search = "",
            string status = "All",
            int rating = 0)
        {
            var query = _context.Reviews
                .Include(r => r.CustomerProfile)
                .Include(r => r.Product)
                    .ThenInclude(p => p!.VendorProfile)
                .Include(r => r.Product)
                    .ThenInclude(p => p!.Category)
                .AsQueryable();

            
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(r =>
                    (r.Comment != null &&
                     r.Comment.Contains(search)) ||

                    (r.CustomerProfile != null &&
                     r.CustomerProfile.FullName.Contains(search)) ||

                    (r.Product != null &&
                     r.Product.ProductName.Contains(search)) ||

                    (r.Product != null &&
                     r.Product.VendorProfile != null &&
                     r.Product.VendorProfile.FarmName.Contains(search)));
            }

          
            if (status == "Pending")
            {
                query = query.Where(r => !r.IsApproved);
            }
            else if (status == "Approved")
            {
                query = query.Where(r => r.IsApproved);
            }

           
            if (rating >= 1 && rating <= 5)
            {
                query = query.Where(r => r.Rating == rating);
            }

            var reviews = await query
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            ViewBag.Search = search;
            ViewBag.Status = status;
            ViewBag.Rating = rating;

         
            ViewBag.TotalReviews =
                await _context.Reviews.CountAsync();

            ViewBag.PendingReviews =
                await _context.Reviews.CountAsync(r => !r.IsApproved);

            ViewBag.ApprovedReviews =
                await _context.Reviews.CountAsync(r => r.IsApproved);

            ViewBag.FiveStarReviews =
                await _context.Reviews.CountAsync(r => r.Rating == 5);

            return View(reviews);
        }


       

        [HttpGet]
        public async Task<IActionResult> ReviewModerationDetails(int id)
        {
            var review = await _context.Reviews
                .Include(r => r.CustomerProfile)
                .Include(r => r.Product)
                    .ThenInclude(p => p!.VendorProfile)
                .Include(r => r.Product)
                    .ThenInclude(p => p!.Category)
                .Include(r => r.ReviewResponses)
                    .ThenInclude(rr => rr.User)
                .FirstOrDefaultAsync(r => r.ReviewId == id);

            if (review == null)
            {
                return NotFound();
            }

            return View(review);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveReview(int id)
        {
            try
            {
                var review = await _context.Reviews
                    .FirstOrDefaultAsync(r => r.ReviewId == id);

                if (review == null)
                {
                    TempData["Error"] = "Review not found.";

                    return RedirectToAction(
                        nameof(ReviewModeration));
                }

                review.IsApproved = true;

                await _context.SaveChangesAsync();

                TempData["Success"] =
                    "Review approved successfully.";
            }
            catch
            {
                TempData["Error"] =
                    "Unable to approve review. Please try again.";
            }

            return RedirectToAction(
                nameof(ReviewModeration));
        }


       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectReview(int id)
        {
            try
            {
                var review = await _context.Reviews
                    .FirstOrDefaultAsync(r => r.ReviewId == id);

                if (review == null)
                {
                    TempData["Error"] = "Review not found.";

                    return RedirectToAction(
                        nameof(ReviewModeration));
                }

                review.IsApproved = false;

                await _context.SaveChangesAsync();

                TempData["Success"] =
                    "Review rejected successfully.";
            }
            catch
            {
                TempData["Error"] =
                    "Unable to reject review. Please try again.";
            }

            return RedirectToAction(
                nameof(ReviewModeration));
        }


       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveReview(int id)
        {
            try
            {
                var review = await _context.Reviews
                    .FirstOrDefaultAsync(r =>
                        r.ReviewId == id);

                if (review == null)
                {
                    TempData["Error"] =
                        "Review not found.";

                    return RedirectToAction(
                        nameof(ReviewModeration));
                }

              

                _context.Reviews.Remove(review);

                await _context.SaveChangesAsync();

                TempData["Success"] =
                    "Review removed successfully.";
            }
            catch
            {
                TempData["Error"] =
                    "Unable to remove review. Please try again.";
            }

            return RedirectToAction(
                nameof(ReviewModeration));
        }

        [HttpGet]
        public async Task<IActionResult> Orders(
     string search = "",
     string status = "All")
        {
            var query = _context.Orders
                .Include(o => o.CustomerProfile)
                .Include(o => o.PickupSlot)
                    .ThenInclude(p => p!.VendorProfile)
                .Include(o => o.OrderItems)
                    .ThenInclude(i => i.Product)
                        .ThenInclude(p => p!.VendorProfile)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(o =>
                    o.OrderNumber.Contains(search) ||

                    (o.CustomerProfile != null &&
                     o.CustomerProfile.FullName.Contains(search)) ||

                    (o.PickupSlot != null &&
                     o.PickupSlot.VendorProfile != null &&
                     o.PickupSlot.VendorProfile.FarmName.Contains(search)));
            }

            if (status != "All")
            {
                query = query.Where(o =>
                    o.Status == status);
            }

            var orders = await query
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            ViewBag.Search = search;
            ViewBag.Status = status;

            ViewBag.TotalOrders =
                await _context.Orders.CountAsync();

            ViewBag.PendingOrders =
                await _context.Orders.CountAsync(o =>
                    o.Status == "Pending");

            ViewBag.VendorApprovedOrders =
                await _context.Orders.CountAsync(o =>
                    o.Status == "VendorApproved");

            ViewBag.ApprovedOrders =
                await _context.Orders.CountAsync(o =>
                    o.Status == "Approved");

            ViewBag.VendorRejectedOrders =
                await _context.Orders.CountAsync(o =>
                    o.Status == "VendorRejected");

            ViewBag.AdminRejectedOrders =
                await _context.Orders.CountAsync(o =>
                    o.Status == "AdminRejected");

            return View(orders);
        }


        [HttpGet]
        public async Task<IActionResult> OrderDetails(int id)
        {
            var order = await _context.Orders

                .Include(o => o.CustomerProfile)

                .Include(o => o.PickupSlot)
                    .ThenInclude(p => p!.VendorProfile)

                .Include(o => o.OrderItems)
                    .ThenInclude(i => i.Product)
                        .ThenInclude(p => p!.Category)

                .FirstOrDefaultAsync(o =>
                    o.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateOrderStatus(
            int id,
            string status)
        {
            try
            {
                var order = await _context.Orders
                    .FirstOrDefaultAsync(o =>
                        o.OrderId == id);

                if (order == null)
                {
                    TempData["Error"] =
                        "Order not found.";

                    return RedirectToAction(
                        nameof(Orders));
                }

                var allowedStatuses = new[]
                {
            "Approved",
            "AdminRejected"
        };

                if (!allowedStatuses.Contains(status))
                {
                    TempData["Error"] =
                        "Invalid order status.";

                    return RedirectToAction(
                        nameof(OrderDetails),
                        new { id });
                }

                if (order.Status != "VendorApproved")
                {
                    TempData["Error"] =
                        "Only vendor-approved orders can receive final admin approval.";

                    return RedirectToAction(
                        nameof(OrderDetails),
                        new { id });
                }

                order.Status = status;

                await _context.SaveChangesAsync();

                if (status == "Approved")
                {
                    TempData["Success"] =
                        $"Order #{order.OrderNumber} has been finally approved. Customer can now see it as Approved.";
                }
                else
                {
                    TempData["Success"] =
                        $"Order #{order.OrderNumber} has been rejected by admin.";
                }
            }
            catch
            {
                TempData["Error"] =
                    "Unable to update order status. Please try again.";
            }

            return RedirectToAction(
                nameof(OrderDetails),
                new { id });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveOrder(int id)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                    .FirstOrDefaultAsync(o =>
                        o.OrderId == id);

                if (order == null)
                {
                    TempData["Error"] =
                        "Order not found.";

                    return RedirectToAction(
                        nameof(Orders));
                }

                if (order.Status == "Approved")
                {
                    TempData["Error"] =
                        "Approved orders cannot be removed.";

                    return RedirectToAction(
                        nameof(Orders));
                }

                _context.Orders.Remove(order);

                await _context.SaveChangesAsync();

                TempData["Success"] =
                    "Order removed successfully.";
            }
            catch
            {
                TempData["Error"] =
                    "Unable to remove order. Please try again.";
            }

            return RedirectToAction(
                nameof(Orders));
        }



        [HttpGet]
        public async Task<IActionResult> PickupSlots(
            string search = "",
            string status = "All")
        {
            var query = _context.PickupSlots
                .Include(p => p.VendorProfile)
                .Include(p => p.Orders)
                .AsQueryable();

        
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(p =>
                    (p.VendorProfile != null &&
                     p.VendorProfile.FarmName.Contains(search)) ||
                    (p.VendorProfile != null &&
                     p.VendorProfile.City.Contains(search)));
            }

        
            if (status == "Available")
            {
                query = query.Where(p => p.IsAvailable);
            }
            else if (status == "Unavailable")
            {
                query = query.Where(p => !p.IsAvailable);
            }

            var pickupSlots = await query
                .OrderByDescending(p => p.PickupDate)
                .ThenBy(p => p.StartTime)
                .ToListAsync();

            ViewBag.Search = search;
            ViewBag.Status = status;

        
            ViewBag.TotalPickupSlots =
                await _context.PickupSlots.CountAsync();

            ViewBag.AvailablePickupSlots =
                await _context.PickupSlots.CountAsync(p =>
                    p.IsAvailable);

            ViewBag.UnavailablePickupSlots =
                await _context.PickupSlots.CountAsync(p =>
                    !p.IsAvailable);

            ViewBag.TotalBookedOrders =
                await _context.Orders.CountAsync(o =>
                    o.PickupSlotId != null);

            return View(pickupSlots);
        }


        [HttpGet]
        public async Task<IActionResult> PickupSlotDetails(int id)
        {
            var pickupSlot = await _context.PickupSlots
                .Include(p => p.VendorProfile)
                .Include(p => p.Orders)
                    .ThenInclude(o => o.CustomerProfile)
                .Include(p => p.Orders)
                    .ThenInclude(o => o.OrderItems)
                        .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(p =>
                    p.PickupSlotId == id);

            if (pickupSlot == null)
            {
                return NotFound();
            }

            return View(pickupSlot);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePickupSlotStatus(
            int id,
            bool isAvailable)
        {
            try
            {
                var pickupSlot = await _context.PickupSlots
                    .FirstOrDefaultAsync(p =>
                        p.PickupSlotId == id);

                if (pickupSlot == null)
                {
                    TempData["Error"] =
                        "Pickup slot not found.";

                    return RedirectToAction(
                        nameof(PickupSlots));
                }

                pickupSlot.IsAvailable = isAvailable;

                await _context.SaveChangesAsync();

                TempData["Success"] =
                    isAvailable
                        ? "Pickup slot marked as available."
                        : "Pickup slot marked as unavailable.";
            }
            catch
            {
                TempData["Error"] =
                    "Unable to update pickup slot status. Please try again.";
            }

            return RedirectToAction(
                nameof(PickupSlots));
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemovePickupSlot(int id)
        {
            try
            {
                var pickupSlot = await _context.PickupSlots
                    .Include(p => p.Orders)
                    .FirstOrDefaultAsync(p =>
                        p.PickupSlotId == id);

                if (pickupSlot == null)
                {
                    TempData["Error"] =
                        "Pickup slot not found.";

                    return RedirectToAction(
                        nameof(PickupSlots));
                }

                if (pickupSlot.Orders != null &&
                    pickupSlot.Orders.Any())
                {
                    TempData["Error"] =
                        "This pickup slot cannot be deleted because orders are linked to it.";

                    return RedirectToAction(
                        nameof(PickupSlots));
                }

                _context.PickupSlots.Remove(pickupSlot);

                await _context.SaveChangesAsync();

                TempData["Success"] =
                    "Pickup slot removed successfully.";
            }
            catch
            {
                TempData["Error"] =
                    "Unable to remove pickup slot. Please try again.";
            }

            return RedirectToAction(
                nameof(PickupSlots));
        }

       


        [HttpGet]
        public async Task<IActionResult> Notifications(
            string search = "",
            string type = "All")
        {
            var query = _context.Notifications
                .Include(n => n.User)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(n =>
                    n.Title.Contains(search) ||
                    n.Message.Contains(search) ||
                    n.NotificationType.Contains(search) ||
                    (n.User != null &&
                     n.User.FullName.Contains(search)));
            }

            if (type != "All")
            {
                query = query.Where(n =>
                    n.NotificationType == type);
            }

            var notifications = await query
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            ViewBag.Search = search;
            ViewBag.Type = type;

            ViewBag.TotalNotifications =
                await _context.Notifications.CountAsync();

            ViewBag.UnreadNotifications =
                await _context.Notifications.CountAsync(n =>
                    !n.IsRead);

            ViewBag.ReadNotifications =
                await _context.Notifications.CountAsync(n =>
                    n.IsRead);

            ViewBag.Announcements =
                await _context.Notifications.CountAsync(n =>
                    n.NotificationType == "Announcement");

            return View(notifications);
        }


        [HttpGet]
        public IActionResult NotificationCreate()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NotificationCreate(
            string title,
            string message,
            string notificationType,
            string audience)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                ModelState.AddModelError(
                    "title",
                    "Title is required.");
            }

            if (string.IsNullOrWhiteSpace(message))
            {
                ModelState.AddModelError(
                    "message",
                    "Message is required.");
            }

            if (string.IsNullOrWhiteSpace(notificationType))
            {
                ModelState.AddModelError(
                    "notificationType",
                    "Notification type is required.");
            }

            if (string.IsNullOrWhiteSpace(audience))
            {
                ModelState.AddModelError(
                    "audience",
                    "Please select an audience.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Title = title;
                ViewBag.Message = message;
                ViewBag.NotificationType = notificationType;
                ViewBag.Audience = audience;

                return View();
            }

            try
            {
                title = title.Trim();
                message = message.Trim();
                notificationType = notificationType.Trim();
                audience = audience.Trim();

                var usersQuery = _context.Users
                    .Where(u => u.IsActive);

                if (audience == "Farmers")
                {
                    usersQuery = usersQuery.Where(u =>
                        u.UserType == "Vendor" ||
                        u.UserType == "Farmer");
                }
                else if (audience == "Customers")
                {
                    usersQuery = usersQuery.Where(u =>
                        u.UserType == "Customer");
                }
                else if (audience != "All")
                {
                    TempData["Error"] =
                        "Invalid audience selected.";

                    return View();
                }

                var users = await usersQuery.ToListAsync();

                if (!users.Any())
                {
                    TempData["Error"] =
                        "No active users were found for the selected audience.";

                    return View();
                }

                var notifications = users.Select(user =>
                    new Notification
                    {
                        UserId = user.Id,
                        Title = title,
                        Message = message,
                        NotificationType = notificationType,
                        IsRead = false,
                        CreatedAt = DateTime.UtcNow
                    })
                    .ToList();

                await _context.Notifications.AddRangeAsync(
                    notifications);

                await _context.SaveChangesAsync();

                TempData["Success"] =
                    $"Announcement published successfully to {notifications.Count} user(s).";

                return RedirectToAction(
                    nameof(Notifications));
            }
            catch
            {
                TempData["Error"] =
                    "Unable to publish notification. Please try again.";

                ViewBag.Title = title;
                ViewBag.Message = message;
                ViewBag.NotificationType = notificationType;
                ViewBag.Audience = audience;

                return View();
            }
        }


        [HttpGet]
        public async Task<IActionResult> NotificationDetails(int id)
        {
            var notification = await _context.Notifications
                .Include(n => n.User)
                .FirstOrDefaultAsync(n =>
                    n.NotificationId == id);

            if (notification == null)
            {
                return NotFound();
            }

            return View(notification);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveNotification(int id)
        {
            try
            {
                var notification =
                    await _context.Notifications
                        .FirstOrDefaultAsync(n =>
                            n.NotificationId == id);

                if (notification == null)
                {
                    TempData["Error"] =
                        "Notification not found.";

                    return RedirectToAction(
                        nameof(Notifications));
                }

                _context.Notifications.Remove(notification);

                await _context.SaveChangesAsync();

                TempData["Success"] =
                    "Notification removed successfully.";
            }
            catch
            {
                TempData["Error"] =
                    "Unable to remove notification. Please try again.";
            }

            return RedirectToAction(
                nameof(Notifications));
        }

       
        [HttpGet]
        public async Task<IActionResult> Reports(
            string reportType = "All",
            DateTime? fromDate = null,
            DateTime? toDate = null)
        {
           
            var today = DateTime.Today;

            var startDate = fromDate?.Date
                ?? new DateTime(today.Year, today.Month, 1);

            var endDate = toDate?.Date
                ?? today;

            if (startDate > endDate)
            {
                TempData["Error"] =
                    "From Date cannot be greater than To Date.";

                startDate = new DateTime(today.Year, today.Month, 1);
                endDate = today;
            }

            

            ViewBag.TotalVendors =
                await _context.VendorProfiles.CountAsync();

            ViewBag.ApprovedVendors =
                await _context.VendorProfiles
                    .CountAsync(v => v.IsApproved);

            ViewBag.PendingVendors =
                await _context.VendorProfiles
                    .CountAsync(v => !v.IsApproved);


            ViewBag.TotalCustomers =
                await _context.CustomerProfiles.CountAsync();


            ViewBag.TotalMarkets =
                await _context.Markets.CountAsync();

            ViewBag.ActiveMarkets =
                await _context.Markets
                    .CountAsync(m => m.IsActive);


            ViewBag.TotalProducts =
                await _context.Products.CountAsync();

            ViewBag.ApprovedProducts =
                await _context.Products
                    .CountAsync(p =>
                        p.ModerationStatus == "Approved");

            ViewBag.PendingProducts =
                await _context.Products
                    .CountAsync(p =>
                        p.ModerationStatus == "Pending");

            ViewBag.RejectedProducts =
                await _context.Products
                    .CountAsync(p =>
                        p.ModerationStatus == "Rejected");


            ViewBag.TotalReviews =
                await _context.Reviews.CountAsync();

            ViewBag.PendingReviews =
                await _context.Reviews
                    .CountAsync(r => !r.IsApproved);

            ViewBag.ApprovedReviews =
                await _context.Reviews
                    .CountAsync(r => r.IsApproved);


           
            var ordersQuery = _context.Orders
                .Include(o => o.CustomerProfile)
                .AsQueryable();

            ordersQuery = ordersQuery.Where(o =>
                o.OrderDate.Date >= startDate &&
                o.OrderDate.Date <= endDate);


            if (reportType == "Pending Orders")
            {
                ordersQuery = ordersQuery.Where(o =>
                    o.Status == "Pending");
            }
            else if (reportType == "Completed Orders")
            {
                ordersQuery = ordersQuery.Where(o =>
                    o.Status == "Completed");
            }
            else if (reportType == "Cancelled Orders")
            {
                ordersQuery = ordersQuery.Where(o =>
                    o.Status == "Cancelled");
            }


            var filteredOrders =
                await ordersQuery
                    .OrderByDescending(o => o.OrderDate)
                    .ToListAsync();


            

            ViewBag.TotalOrders =
                filteredOrders.Count;

            ViewBag.PendingOrders =
                filteredOrders.Count(o =>
                    o.Status == "Pending");

            ViewBag.ConfirmedOrders =
                filteredOrders.Count(o =>
                    o.Status == "Confirmed");

            ViewBag.CompletedOrders =
                filteredOrders.Count(o =>
                    o.Status == "Completed");

            ViewBag.CancelledOrders =
                filteredOrders.Count(o =>
                    o.Status == "Cancelled");


           

            ViewBag.TotalOrderAmount =
                filteredOrders.Sum(o =>
                    o.TotalAmount);


            ViewBag.CompletedOrderAmount =
                filteredOrders
                    .Where(o => o.Status == "Completed")
                    .Sum(o => o.TotalAmount);


            ViewBag.PendingOrderAmount =
                filteredOrders
                    .Where(o => o.Status == "Pending")
                    .Sum(o => o.TotalAmount);


            ViewBag.CancelledOrderAmount =
                filteredOrders
                    .Where(o => o.Status == "Cancelled")
                    .Sum(o => o.TotalAmount);


           

            ViewBag.AverageOrderValue =
                filteredOrders.Any()
                    ? filteredOrders.Average(o =>
                        o.TotalAmount)
                    : 0;



            var orderIds =
                filteredOrders
                    .Select(o => o.OrderId)
                    .ToList();

            var topProducts = await _context.OrderItems
                .Include(i => i.Product)
                .Where(i =>
                    orderIds.Contains(i.OrderId))
                .GroupBy(i => new
                {
                    i.ProductId,
                    ProductName =
                        i.Product != null
                            ? i.Product.ProductName
                            : "Unknown Product"
                })
                .Select(g => new
                {
                    ProductName = g.Key.ProductName,

                    TotalQuantity =
                        g.Sum(x => x.Quantity),

                    TotalSales =
                        g.Sum(x => x.TotalPrice)
                })
                .OrderByDescending(x =>
                    x.TotalQuantity)
                .Take(5)
                .ToListAsync();

            ViewBag.TopProducts =
                topProducts;


           

            var topVendors = await _context.OrderItems
                .Include(i => i.Product)
                    .ThenInclude(p => p!.VendorProfile)
                .Where(i =>
                    orderIds.Contains(i.OrderId) &&
                    i.Product != null &&
                    i.Product.VendorProfile != null)
                .GroupBy(i => new
                {
                    VendorProfileId =
                        i.Product!.VendorProfileId,

                    FarmName =
                        i.Product.VendorProfile!.FarmName
                })
                .Select(g => new
                {
                    FarmName = g.Key.FarmName,

                    TotalItems =
                        g.Sum(x => x.Quantity),

                    TotalSales =
                        g.Sum(x => x.TotalPrice)
                })
                .OrderByDescending(x =>
                    x.TotalSales)
                .Take(5)
                .ToListAsync();

            ViewBag.TopVendors =
                topVendors;


           
            var dailyOrders =
                filteredOrders
                    .GroupBy(o => o.OrderDate.Date)
                    .Select(g => new
                    {
                        Date = g.Key,

                        Orders = g.Count(),

                        Amount =
                            g.Sum(x => x.TotalAmount)
                    })
                    .OrderBy(x => x.Date)
                    .ToList();

            ViewBag.DailyOrders =
                dailyOrders;


            

            ViewBag.RecentOrders =
                filteredOrders
                    .Take(10)
                    .ToList();


           

            ViewBag.ReportHistory =
                await _context.Reports
                    .Include(r => r.GeneratedByUser)
                    .OrderByDescending(r => r.CreatedAt)
                    .Take(10)
                    .ToListAsync();


            ViewBag.ReportType =
                reportType;

            ViewBag.FromDate =
                startDate.ToString("yyyy-MM-dd");

            ViewBag.ToDate =
                endDate.ToString("yyyy-MM-dd");


            return View();
        }


       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateReport(
            string reportType,
            DateTime fromDate,
            DateTime toDate,
            string? description)
        {
            if (fromDate.Date > toDate.Date)
            {
                TempData["Error"] =
                    "From Date cannot be greater than To Date.";

                return RedirectToAction(nameof(Reports));
            }

            try
            {
                var userId =
                    User.FindFirst(
                        System.Security.Claims.ClaimTypes.NameIdentifier)
                    ?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    TempData["Error"] =
                        "Unable to identify the admin user.";

                    return RedirectToAction(nameof(Reports));
                }

                var report = new Report
                {
                    ReportType =
                        string.IsNullOrWhiteSpace(reportType)
                            ? "General"
                            : reportType.Trim(),

                    FromDate = fromDate.Date,

                    ToDate = toDate.Date,

                    Description =
                        string.IsNullOrWhiteSpace(description)
                            ? null
                            : description.Trim(),

                    GeneratedByUserId =
                        userId,

                    CreatedAt =
                        DateTime.UtcNow
                };

                _context.Reports.Add(report);

                await _context.SaveChangesAsync();

                TempData["Success"] =
                    "Report generated successfully.";

            }
            catch
            {
                TempData["Error"] =
                    "Unable to generate report. Please try again.";
            }

            return RedirectToAction(nameof(Reports));
        }


       

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveReport(int id)
        {
            try
            {
                var report =
                    await _context.Reports
                        .FirstOrDefaultAsync(r =>
                            r.ReportId == id);

                if (report == null)
                {
                    TempData["Error"] =
                        "Report not found.";

                    return RedirectToAction(
                        nameof(Reports));
                }

                _context.Reports.Remove(report);

                await _context.SaveChangesAsync();

                TempData["Success"] =
                    "Report history removed successfully.";
            }
            catch
            {
                TempData["Error"] =
                    "Unable to remove report. Please try again.";
            }

            return RedirectToAction(
                nameof(Reports));
        }

       
      

       
    }
}