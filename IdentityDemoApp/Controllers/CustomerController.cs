using IdentityDemoApp.Data;
using IdentityDemoApp.Models;
using IdentityDemoApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace IdentityDemoApp.Controllers
{
    [Authorize(Roles = "Customer")]
    public class CustomerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly EmailService _emailService;

        public CustomerController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            EmailService emailService)
        {
            _context = context;
            _userManager = userManager;
            _emailService = emailService;
        }
        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Account");

            var customer = await _context.CustomerProfiles
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (customer == null)
                return NotFound();

            var orderCount = await _context.Orders
                .CountAsync(o => o.CustomerProfileId == customer.CustomerProfileId);

            var favouriteProducts = await _context.FavouriteProducts
                .CountAsync(f => f.CustomerProfileId == customer.CustomerProfileId);

            var favouriteFarmers = await _context.FavouriteVendors
                .CountAsync(f => f.CustomerProfileId == customer.CustomerProfileId);

            var reviewCount = await _context.Reviews
                .CountAsync(r => r.CustomerProfileId == customer.CustomerProfileId);

            ViewBag.OrderCount = orderCount;
            ViewBag.FavouriteProducts = favouriteProducts;
            ViewBag.FavouriteFarmers = favouriteFarmers;
            ViewBag.ReviewCount = reviewCount;

            return View(customer);
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Account");

            var customer = await _context.CustomerProfiles
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (customer == null)
                return NotFound();

            return View(customer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(CustomerProfile model)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Account");

            var customer = await _context.CustomerProfiles
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (customer == null)
                return NotFound();

            if (!ModelState.IsValid)
                return View(model);

            customer.FullName = model.FullName;
            customer.ContactNumber = model.ContactNumber;
            customer.Address = model.Address;
            customer.City = model.City;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Profile updated successfully.";

            return RedirectToAction(nameof(Profile));
        }

        // Customer - Browse Markets
        public async Task<IActionResult> BrowseMarkets(string? search, string? city)
        {
            var query = _context.Markets
                .AsNoTracking()
                .Where(m => m.IsActive)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(m =>
                    m.MarketName.Contains(search) ||
                    m.Location.Contains(search) ||
                    m.City.Contains(search));
            }

            if (!string.IsNullOrWhiteSpace(city))
            {
                city = city.Trim();

                query = query.Where(m => m.City == city);
            }

            var markets = await query
                .OrderBy(m => m.MarketName)
                .ToListAsync();

            ViewBag.Search = search ?? "";
            ViewBag.City = city ?? "";

            ViewBag.Cities = await _context.Markets
                .AsNoTracking()
                .Where(m => m.IsActive && !string.IsNullOrEmpty(m.City))
                .Select(m => m.City)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            return View(markets);
        }

        [Authorize]
        public async Task<IActionResult> BrowseVendors(string? search, string? city)
        {
            var query = _context.VendorProfiles
                .AsNoTracking()
                .Where(v => v.IsApproved);

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(v =>
                    v.FarmName.Contains(search) ||
                    v.Address.Contains(search) ||
                    v.City.Contains(search) ||
                    v.Province.Contains(search));
            }

            if (!string.IsNullOrWhiteSpace(city))
            {
                city = city.Trim();
                query = query.Where(v => v.City == city);
            }

            var farmers = await query
                .OrderBy(v => v.FarmName)
                .ToListAsync();

            ViewBag.Search = search;
            ViewBag.City = city;

            ViewBag.Cities = await _context.VendorProfiles
                .AsNoTracking()
                .Where(v => v.IsApproved && !string.IsNullOrEmpty(v.City))
                .Select(v => v.City)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            return View(farmers);
        }

        public async Task<IActionResult> FavoriteFarmers()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Account");

            var customer = await _context.CustomerProfiles
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (customer == null)
                return NotFound();

            var favouriteFarmers = await _context.FavouriteVendors
                .AsNoTracking()
                .Include(f => f.VendorProfile)
                .Where(f =>
                    f.CustomerProfileId == customer.CustomerProfileId &&
                    f.VendorProfile != null &&
                    f.VendorProfile.IsApproved)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();

            return View(favouriteFarmers);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleFavoriteVendor(int vendorProfileId)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Account");

            var customer = await _context.CustomerProfiles
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (customer == null)
                return NotFound();

            var vendor = await _context.VendorProfiles
                .FirstOrDefaultAsync(v =>
                    v.VendorProfileId == vendorProfileId &&
                    v.IsApproved);

            if (vendor == null)
                return NotFound();

            var existingFavorite = await _context.FavouriteVendors
                .FirstOrDefaultAsync(f =>
                    f.CustomerProfileId == customer.CustomerProfileId &&
                    f.VendorProfileId == vendorProfileId);

            if (existingFavorite != null)
            {
                _context.FavouriteVendors.Remove(existingFavorite);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Farmer removed from favorites.";
            }
            else
            {
                var favorite = new FavouriteVendor
                {
                    CustomerProfileId = customer.CustomerProfileId,
                    VendorProfileId = vendorProfileId,
                    CreatedAt = DateTime.UtcNow
                };

                _context.FavouriteVendors.Add(favorite);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Farmer added to favorites.";
            }

            return RedirectToAction(nameof(FavoriteFarmers));
        }

        public async Task<IActionResult> FavoriteProducts()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Account");

            var customer = await _context.CustomerProfiles
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (customer == null)
                return NotFound();

            var favoriteProducts = await _context.FavouriteProducts
                .AsNoTracking()
                .Include(f => f.Product)
                    .ThenInclude(p => p!.VendorProfile)
                .Include(f => f.Product)
                    .ThenInclude(p => p!.Category)
                .Where(f =>
                    f.CustomerProfileId == customer.CustomerProfileId &&
                    f.Product != null &&
                    f.Product.IsAvailable &&
                    f.Product.ModerationStatus == "Approved")
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();

            return View(favoriteProducts);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleFavoriteProduct(int productId)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Account");

            var customer = await _context.CustomerProfiles
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (customer == null)
                return NotFound();

            var product = await _context.Products
                .FirstOrDefaultAsync(p =>
                    p.ProductId == productId &&
                    p.IsAvailable &&
                    p.ModerationStatus == "Approved");

            if (product == null)
                return NotFound();

            var existingFavorite = await _context.FavouriteProducts
                .FirstOrDefaultAsync(f =>
                    f.CustomerProfileId == customer.CustomerProfileId &&
                    f.ProductId == productId);

            if (existingFavorite != null)
            {
                _context.FavouriteProducts.Remove(existingFavorite);

                await _context.SaveChangesAsync();

                TempData["Success"] = "Product removed from favorites.";
            }
            else
            {
                var favorite = new FavouriteProduct
                {
                    CustomerProfileId = customer.CustomerProfileId,
                    ProductId = productId,
                    CreatedAt = DateTime.UtcNow
                };

                _context.FavouriteProducts.Add(favorite);

                await _context.SaveChangesAsync();

                TempData["Success"] = "Product added to favorites.";
            }

            return RedirectToAction(nameof(FavoriteProducts));
        }

        // ============================================================
        // CUSTOMER CART
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> Cart()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Account");

            var customer = await _context.CustomerProfiles
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (customer == null)
                return NotFound();

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                        .ThenInclude(p => p!.VendorProfile)
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                        .ThenInclude(p => p!.Category)
                .FirstOrDefaultAsync(c =>
                    c.CustomerProfileId == customer.CustomerProfileId);

            // Customer ka cart nahi bana hua
            if (cart == null)
            {
                cart = new Cart
                {
                    CustomerProfileId = customer.CustomerProfileId,
                    TotalAmount = 0,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            // Sirf valid/available products show hon
            cart.CartItems = cart.CartItems
                .Where(ci =>
                    ci.Product != null &&
                    ci.Product.IsAvailable &&
                    ci.Product.ModerationStatus == "Approved")
                .ToList();

            // Cart total automatically calculate
            cart.TotalAmount = cart.CartItems.Sum(ci => ci.TotalPrice);

            return View(cart);
        }


        // ============================================================
        // ADD PRODUCT TO CART
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(int productId, decimal quantity = 1)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Account");

            var customer = await _context.CustomerProfiles
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (customer == null)
                return NotFound();

            if (quantity <= 0)
                quantity = 1;

            var product = await _context.Products
                .FirstOrDefaultAsync(p =>
                    p.ProductId == productId &&
                    p.IsAvailable &&
                    p.ModerationStatus == "Approved");

            if (product == null)
            {
                TempData["Error"] = "Product is not available.";
                return RedirectToAction("Products", "Home");
            }

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c =>
                    c.CustomerProfileId == customer.CustomerProfileId);

            // Agar cart nahi hai to create karo
            if (cart == null)
            {
                cart = new Cart
                {
                    CustomerProfileId = customer.CustomerProfileId,
                    TotalAmount = 0,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            // Check karo product already cart mein hai ya nahi
            var existingItem = cart.CartItems
                .FirstOrDefault(ci => ci.ProductId == productId);

            if (existingItem != null)
            {
                // Existing quantity mein new quantity add
                existingItem.Quantity += quantity;

                // Automatic calculation
                existingItem.UnitPrice = product.Price;
                existingItem.TotalPrice =
                    existingItem.Quantity * existingItem.UnitPrice;
            }
            else
            {
                // New cart item
                var cartItem = new CartItem
                {
                    CartId = cart.CartId,
                    ProductId = product.ProductId,
                    Quantity = quantity,
                    UnitPrice = product.Price,

                    // Formula:
                    // Quantity × UnitPrice
                    TotalPrice = quantity * product.Price
                };

                _context.CartItems.Add(cartItem);
            }

            await _context.SaveChangesAsync();

            // Cart total automatically calculate
            cart.TotalAmount = await _context.CartItems
                .Where(ci => ci.CartId == cart.CartId)
                .SumAsync(ci => ci.TotalPrice);

            cart.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["Success"] = $"{product.ProductName} added to cart.";

            return RedirectToAction(nameof(Cart));
        }


        // ============================================================
        // UPDATE CART QUANTITY
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateCart(int cartItemId, decimal quantity)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Account");

            var customer = await _context.CustomerProfiles
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (customer == null)
                return NotFound();

            var cartItem = await _context.CartItems
                .Include(ci => ci.Cart)
                .Include(ci => ci.Product)
                .FirstOrDefaultAsync(ci =>
                    ci.CartItemId == cartItemId &&
                    ci.Cart!.CustomerProfileId == customer.CustomerProfileId);

            if (cartItem == null)
                return NotFound();

            // Quantity 0 ya negative ho to item remove
            if (quantity <= 0)
            {
                _context.CartItems.Remove(cartItem);
            }
            else
            {
                cartItem.Quantity = quantity;

                // Product ki current price
                cartItem.UnitPrice = cartItem.Product?.Price ?? cartItem.UnitPrice;

                // Formula:
                // Quantity × UnitPrice
                cartItem.TotalPrice =
                    cartItem.Quantity * cartItem.UnitPrice;
            }

            await _context.SaveChangesAsync();

            // Recalculate Cart Total
            var cart = cartItem.Cart;

            if (cart != null)
            {
                cart.TotalAmount = await _context.CartItems
                    .Where(ci => ci.CartId == cart.CartId)
                    .SumAsync(ci => ci.TotalPrice);

                cart.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
            }

            TempData["Success"] = "Cart updated successfully.";

            return RedirectToAction(nameof(Cart));
        }


        // ============================================================
        // REMOVE ITEM FROM CART
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromCart(int cartItemId)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Account");

            var customer = await _context.CustomerProfiles
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (customer == null)
                return NotFound();

            var cartItem = await _context.CartItems
                .Include(ci => ci.Cart)
                .FirstOrDefaultAsync(ci =>
                    ci.CartItemId == cartItemId &&
                    ci.Cart!.CustomerProfileId == customer.CustomerProfileId);

            if (cartItem == null)
                return NotFound();

            var cart = cartItem.Cart;

            _context.CartItems.Remove(cartItem);

            await _context.SaveChangesAsync();

            if (cart != null)
            {
                // Recalculate Cart Total
                cart.TotalAmount = await _context.CartItems
                    .Where(ci => ci.CartId == cart.CartId)
                    .SumAsync(ci => ci.TotalPrice);

                cart.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
            }

            TempData["Success"] = "Product removed from cart.";

            return RedirectToAction(nameof(Cart));
        }


        // ============================================================
        // CLEAR ENTIRE CART
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ClearCart()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Account");

            var customer = await _context.CustomerProfiles
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (customer == null)
                return NotFound();

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c =>
                    c.CustomerProfileId == customer.CustomerProfileId);

            if (cart == null)
            {
                return RedirectToAction(nameof(Cart));
            }

            _context.CartItems.RemoveRange(cart.CartItems);

            cart.TotalAmount = 0;
            cart.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Cart cleared successfully.";

            return RedirectToAction(nameof(Cart));
        }

        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Account");

            var customer = await _context.CustomerProfiles
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (customer == null)
                return NotFound();

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                        .ThenInclude(p => p!.VendorProfile)
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                        .ThenInclude(p => p!.Category)
                .FirstOrDefaultAsync(c =>
                    c.CustomerProfileId == customer.CustomerProfileId);

            if (cart == null || !cart.CartItems.Any())
            {
                TempData["Error"] = "Your cart is empty.";
                return RedirectToAction(nameof(Cart));
            }

            cart.CartItems = cart.CartItems
                .Where(ci =>
                    ci.Product != null &&
                    ci.Product.IsAvailable &&
                    ci.Product.ModerationStatus == "Approved")
                .ToList();

            if (!cart.CartItems.Any())
            {
                TempData["Error"] = "No available products found in your cart.";
                return RedirectToAction(nameof(Cart));
            }

            foreach (var item in cart.CartItems)
            {
                item.UnitPrice = item.Product!.Price;
                item.TotalPrice = item.Quantity * item.UnitPrice;
            }

            cart.TotalAmount = cart.CartItems.Sum(ci => ci.TotalPrice);

            var pickupSlots = await _context.PickupSlots
                .AsNoTracking()
                .Where(p => p.IsAvailable)
                .OrderBy(p => p.PickupDate)
                .ThenBy(p => p.StartTime)
                .ToListAsync();

            ViewBag.PickupSlots = pickupSlots;

            return View(cart);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(
    int? pickupSlotId,
    string? notes)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Your session has expired. Please login again."
                });
            }

            var customer = await _context.CustomerProfiles
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (customer == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Customer profile was not found."
                });
            }

            var customerEmail = user.Email;

            if (string.IsNullOrWhiteSpace(customerEmail))
            {
                return Json(new
                {
                    success = false,
                    message = "Your email address was not found."
                });
            }

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                        .ThenInclude(p => p!.VendorProfile)
                            .ThenInclude(v => v!.User)
                .FirstOrDefaultAsync(c =>
                    c.CustomerProfileId == customer.CustomerProfileId);

            if (cart == null || !cart.CartItems.Any())
            {
                return Json(new
                {
                    success = false,
                    message = "Your cart is empty."
                });
            }

            var validItems = cart.CartItems
                .Where(ci =>
                    ci.Product != null &&
                    ci.Product.IsAvailable &&
                    ci.Product.ModerationStatus == "Approved")
                .ToList();

            if (!validItems.Any())
            {
                return Json(new
                {
                    success = false,
                    message = "No available products found in your cart."
                });
            }

            if (pickupSlotId.HasValue)
            {
                var pickupSlotExists = await _context.PickupSlots
                    .AnyAsync(p =>
                        p.PickupSlotId == pickupSlotId.Value &&
                        p.IsAvailable);

                if (!pickupSlotExists)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Selected pickup slot is not available."
                    });
                }
            }

            foreach (var item in validItems)
            {
                item.UnitPrice = item.Product!.Price;
                item.TotalPrice = item.Quantity * item.UnitPrice;
            }

            var totalAmount = validItems.Sum(i => i.TotalPrice);

            var order = new Order
            {
                OrderNumber =
                    $"ML-{DateTime.UtcNow:yyyyMMddHHmmssfff}",

                CustomerProfileId =
                    customer.CustomerProfileId,

                PickupSlotId =
                    pickupSlotId,

                OrderDate =
                    DateTime.UtcNow,

                Status =
                    "Pending",

                TotalAmount =
                    totalAmount,

                Notes =
                    string.IsNullOrWhiteSpace(notes)
                        ? null
                        : notes.Trim()
            };

            foreach (var cartItem in validItems)
            {
                order.OrderItems.Add(new OrderItem
                {
                    ProductId =
                        cartItem.ProductId,

                    Quantity =
                        cartItem.Quantity,

                    UnitPrice =
                        cartItem.UnitPrice,

                    TotalPrice =
                        cartItem.TotalPrice
                });
            }

            _context.Orders.Add(order);

            _context.CartItems.RemoveRange(cart.CartItems);

            cart.TotalAmount = 0;
            cart.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var pickupSlot = pickupSlotId.HasValue
                ? await _context.PickupSlots
                    .FirstOrDefaultAsync(p =>
                        p.PickupSlotId == pickupSlotId.Value)
                : null;

            try
            {
                var customerName =
                    customer.FullName
                    ?? user.UserName
                    ?? "Customer";

                var customerEmailBody =
                    BuildCustomerOrderEmail(
                        customerName,
                        order.OrderNumber,
                        totalAmount,
                        validItems,
                        pickupSlot
                    );

                await _emailService.SendEmailAsync(
                    customerEmail,
                    $"MarketLink Order Confirmation - {order.OrderNumber}",
                    customerEmailBody
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    "CUSTOMER EMAIL ERROR: " +
                    ex.Message
                );
            }

            try
            {
                var adminUsers =
                    await _userManager
                        .GetUsersInRoleAsync("Admin");

                foreach (var admin in adminUsers)
                {
                    if (string.IsNullOrWhiteSpace(admin.Email))
                        continue;

                    var adminEmailBody =
                        BuildAdminOrderEmail(
                            customer.FullName ?? "Customer",
                            customerEmail,
                            order.OrderNumber,
                            totalAmount,
                            validItems,
                            pickupSlot
                        );

                    await _emailService.SendEmailAsync(
                        admin.Email,
                        $"New MarketLink Order - {order.OrderNumber}",
                        adminEmailBody
                    );
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    "ADMIN EMAIL ERROR: " +
                    ex.Message
                );
            }

            try
            {
                var vendorGroups =
                    validItems
                        .Where(i =>
                            i.Product != null &&
                            i.Product.VendorProfile != null &&
                            i.Product.VendorProfile.User != null)
                        .GroupBy(i =>
                            i.Product!.VendorProfileId);

                foreach (var vendorGroup in vendorGroups)
                {
                    var vendorItems =
                        vendorGroup.ToList();

                    var vendor =
                        vendorItems
                            .First()
                            .Product!
                            .VendorProfile!;

                    var vendorUser =
                        vendor.User;

                    if (vendorUser == null ||
                        string.IsNullOrWhiteSpace(vendorUser.Email))
                    {
                        continue;
                    }

                    var vendorEmailBody =
                        BuildVendorOrderEmail(
                            customer.FullName ?? "Customer",
                            customerEmail,
                            order.OrderNumber,
                            vendorItems,
                            pickupSlot
                        );

                    await _emailService.SendEmailAsync(
                        vendorUser.Email,
                        $"New Order Received - {order.OrderNumber}",
                        vendorEmailBody
                    );
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    "VENDOR EMAIL ERROR: " +
                    ex.Message
                );
            }

            return Json(new
            {
                success = true,

                message =
                    $"Your pre-order {order.OrderNumber} has been placed successfully.",

                orderNumber =
                    order.OrderNumber,

                redirectUrl =
                    Url.Action(nameof(Order))
            });
        }
        public async Task<IActionResult> Order()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Account");

            var customer = await _context.CustomerProfiles
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (customer == null)
                return NotFound();

            var orders = await _context.Orders
                .Where(o => o.CustomerProfileId == customer.CustomerProfileId)
                .Include(o => o.PickupSlot)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View("MyOrders", orders);
        }

        public async Task<IActionResult> OrderDetails(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Account");

            var customer = await _context.CustomerProfiles
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (customer == null)
                return NotFound();

            var order = await _context.Orders
                .Include(o => o.PickupSlot)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                        .ThenInclude(p => p.VendorProfile)
                .FirstOrDefaultAsync(o =>
                    o.OrderId == id &&
                    o.CustomerProfileId == customer.CustomerProfileId);

            if (order == null)
                return NotFound();

            return View(order);
        }

        [HttpGet]
        public async Task<IActionResult> Products(string? search, int? categoryId, int? vendorId)
        {
            var query = _context.Products
                .AsNoTracking()
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
                    p.Unit.Contains(search));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            if (vendorId.HasValue)
            {
                query = query.Where(p => p.VendorProfileId == vendorId.Value);
            }

            var products = await query
                .OrderBy(p => p.ProductName)
                .ToListAsync();

            ViewBag.Search = search ?? "";
            ViewBag.CategoryId = categoryId;
            ViewBag.VendorId = vendorId;

            ViewBag.Categories = await _context.Categories
                .AsNoTracking()
                .OrderBy(c => c.CategoryName)
                .ToListAsync();

            ViewBag.Vendors = await _context.VendorProfiles
                .AsNoTracking()
                .Where(v => v.IsApproved)
                .OrderBy(v => v.FarmName)
                .ToListAsync();

            var user = await _userManager.GetUserAsync(User);

            if (user != null)
            {
                var customer = await _context.CustomerProfiles
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.UserId == user.Id);

                if (customer != null)
                {
                    ViewBag.FavouriteProductIds = await _context.FavouriteProducts
                        .AsNoTracking()
                        .Where(f =>
                            f.CustomerProfileId == customer.CustomerProfileId)
                        .Select(f => f.ProductId)
                        .ToListAsync();
                }
            }

            return View(products);
        }

        [HttpGet]
        public async Task<IActionResult> ProductDetails(int id)
        {
            var product = await _context.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.VendorProfile)
                .FirstOrDefaultAsync(p =>
                    p.ProductId == id &&
                    p.IsAvailable &&
                    p.ModerationStatus == "Approved");

            if (product == null)
                return NotFound();

            var approvedReviews = await _context.Reviews
                .AsNoTracking()
                .Include(r => r.CustomerProfile)
                .Include(r => r.ReviewResponses)
                    .ThenInclude(rr => rr.User)
                .Where(r =>
                    r.ProductId == id &&
                    r.IsApproved)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            ViewBag.Reviews = approvedReviews;

            ViewBag.AverageRating = approvedReviews.Any()
                ? approvedReviews.Average(r => r.Rating)
                : 0;

            ViewBag.ReviewCount = approvedReviews.Count;

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Account");

            var customer = await _context.CustomerProfiles
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (customer == null)
                return NotFound();

            var hasPurchased = await _context.OrderItems
                .AnyAsync(oi =>
                    oi.ProductId == id &&
                    oi.Order != null &&
                    oi.Order.CustomerProfileId == customer.CustomerProfileId);

            var hasReviewed = await _context.Reviews
                .AnyAsync(r =>
                    r.ProductId == id &&
                    r.CustomerProfileId == customer.CustomerProfileId);

            var isFavourite = await _context.FavouriteProducts
                .AnyAsync(f =>
                    f.ProductId == id &&
                    f.CustomerProfileId == customer.CustomerProfileId);

            ViewBag.HasPurchased = hasPurchased;
            ViewBag.HasReviewed = hasReviewed;
            ViewBag.IsFavourite = isFavourite;

            return View(product);
        }

        [HttpGet]
        public async Task<IActionResult> FavoriteMarkets()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Account");

            var customer = await _context.CustomerProfiles
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (customer == null)
                return NotFound();

            var markets = await _context.FavouriteMarkets
                .Where(f => f.CustomerProfileId == customer.CustomerProfileId)
                .Include(f => f.Market)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();

            return View(markets);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleFavoriteMarket(int marketId)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Account");

            var customer = await _context.CustomerProfiles
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (customer == null)
                return NotFound();

            var market = await _context.Markets
                .FirstOrDefaultAsync(m => m.MarketId == marketId && m.IsActive);

            if (market == null)
            {
                TempData["ErrorMessage"] = "Market not found.";
                return RedirectToAction(nameof(FavoriteMarkets));
            }

            var existing = await _context.FavouriteMarkets
                .FirstOrDefaultAsync(f =>
                    f.CustomerProfileId == customer.CustomerProfileId &&
                    f.MarketId == marketId);

            if (existing != null)
            {
                _context.FavouriteMarkets.Remove(existing);

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] =
                    $"{market.MarketName} removed from saved markets.";
            }
            else
            {
                var favouriteMarket = new FavouriteMarket
                {
                    CustomerProfileId = customer.CustomerProfileId,
                    MarketId = marketId,
                    CreatedAt = DateTime.UtcNow
                };

                _context.FavouriteMarkets.Add(favouriteMarket);

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] =
                    $"{market.MarketName} saved successfully.";
            }

            return RedirectToAction(nameof(FavoriteMarkets));
        }

        [HttpGet]
        public async Task<IActionResult> Reviews()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Account");

            var customer = await _context.CustomerProfiles
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (customer == null)
                return NotFound();

            var reviews = await _context.Reviews
                .AsNoTracking()
                .Include(r => r.Product)
                    .ThenInclude(p => p!.VendorProfile)
                .Include(r => r.ReviewResponses)
                    .ThenInclude(rr => rr.User)
                .Where(r => r.CustomerProfileId == customer.CustomerProfileId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            var reviewedProductIds = reviews
                .Select(r => r.ProductId)
                .ToHashSet();

            var purchasedProductIds = await _context.OrderItems
                .AsNoTracking()
                .Where(oi =>
                    oi.Order != null &&
                    oi.Order.CustomerProfileId == customer.CustomerProfileId)
                .Select(oi => oi.ProductId)
                .Distinct()
                .ToListAsync();

            ViewBag.ReviewedProductIds = reviewedProductIds;
            ViewBag.PurchasedProductIds = purchasedProductIds;

            return View(reviews);
        }

        [HttpGet]
        public async Task<IActionResult> CreateReview(int productId)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Account");

            var customer = await _context.CustomerProfiles
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (customer == null)
                return NotFound();

            var product = await _context.Products
                .AsNoTracking()
                .Include(p => p.VendorProfile)
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p =>
                    p.ProductId == productId &&
                    p.IsAvailable &&
                    p.ModerationStatus == "Approved");

            if (product == null)
            {
                TempData["Error"] = "Product not found or is no longer available.";
                return RedirectToAction(nameof(Reviews));
            }

            var purchased = await _context.OrderItems
                .AnyAsync(oi =>
                    oi.ProductId == productId &&
                    oi.Order != null &&
                    oi.Order.CustomerProfileId == customer.CustomerProfileId);

            if (!purchased)
            {
                TempData["Error"] =
                    "You can only review products that you have ordered.";

                return RedirectToAction(nameof(Reviews));
            }

            var alreadyReviewed = await _context.Reviews
                .AnyAsync(r =>
                    r.CustomerProfileId == customer.CustomerProfileId &&
                    r.ProductId == productId);

            if (alreadyReviewed)
            {
                TempData["Error"] = "You have already reviewed this product.";
                return RedirectToAction(nameof(Reviews));
            }

            var review = new Review
            {
                CustomerProfileId = customer.CustomerProfileId,
                ProductId = productId,
                Rating = 5
            };

            ViewBag.Product = product;

            return View(review);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateReview(Review model)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Account");

            var customer = await _context.CustomerProfiles
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (customer == null)
                return NotFound();

            if (model.Rating < 1 || model.Rating > 5)
            {
                ModelState.AddModelError(
                    nameof(model.Rating),
                    "Please select a rating between 1 and 5.");
            }

            var product = await _context.Products
                .AsNoTracking()
                .Include(p => p.VendorProfile)
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p =>
                    p.ProductId == model.ProductId &&
                    p.IsAvailable &&
                    p.ModerationStatus == "Approved");

            if (product == null)
            {
                TempData["Error"] = "Product not found.";
                return RedirectToAction(nameof(Reviews));
            }

            var purchased = await _context.OrderItems
                .AnyAsync(oi =>
                    oi.ProductId == model.ProductId &&
                    oi.Order != null &&
                    oi.Order.CustomerProfileId == customer.CustomerProfileId);

            if (!purchased)
            {
                TempData["Error"] =
                    "You can only review products that you have ordered.";

                return RedirectToAction(nameof(Reviews));
            }

            var existingReview = await _context.Reviews
                .FirstOrDefaultAsync(r =>
                    r.CustomerProfileId == customer.CustomerProfileId &&
                    r.ProductId == model.ProductId);

            if (existingReview != null)
            {
                TempData["Error"] = "You have already reviewed this product.";
                return RedirectToAction(nameof(Reviews));
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Product = product;
                return View(model);
            }

            var review = new Review
            {
                CustomerProfileId = customer.CustomerProfileId,
                ProductId = model.ProductId,
                Rating = model.Rating,
                Comment = string.IsNullOrWhiteSpace(model.Comment)
                    ? null
                    : model.Comment.Trim(),
                CreatedAt = DateTime.UtcNow,
                IsApproved = false
            };

            _context.Reviews.Add(review);

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Your review has been submitted and is waiting for approval.";

            return RedirectToAction(nameof(Reviews));
        }

        [HttpGet]
        public async Task<IActionResult> EditReview(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Account");

            var customer = await _context.CustomerProfiles
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (customer == null)
                return NotFound();

            var review = await _context.Reviews
                .Include(r => r.Product)
                    .ThenInclude(p => p!.VendorProfile)
                .FirstOrDefaultAsync(r =>
                    r.ReviewId == id &&
                    r.CustomerProfileId == customer.CustomerProfileId);

            if (review == null)
                return NotFound();

            return View(review);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditReview(Review model)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Account");

            var customer = await _context.CustomerProfiles
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (customer == null)
                return NotFound();

            var review = await _context.Reviews
                .Include(r => r.Product)
                    .ThenInclude(p => p!.VendorProfile)
                .FirstOrDefaultAsync(r =>
                    r.ReviewId == model.ReviewId &&
                    r.CustomerProfileId == customer.CustomerProfileId);

            if (review == null)
                return NotFound();

            if (model.Rating < 1 || model.Rating > 5)
            {
                ModelState.AddModelError(
                    nameof(model.Rating),
                    "Please select a rating between 1 and 5.");
            }

            if (!ModelState.IsValid)
                return View(model);

            review.Rating = model.Rating;

            review.Comment = string.IsNullOrWhiteSpace(model.Comment)
                ? null
                : model.Comment.Trim();

            review.IsApproved = false;

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Your review has been updated and sent for approval again.";

            return RedirectToAction(nameof(Reviews));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Account");

            var customer = await _context.CustomerProfiles
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (customer == null)
                return NotFound();

            var review = await _context.Reviews
                .FirstOrDefaultAsync(r =>
                    r.ReviewId == id &&
                    r.CustomerProfileId == customer.CustomerProfileId);

            if (review == null)
                return NotFound();

            _context.Reviews.Remove(review);

            await _context.SaveChangesAsync();

            TempData["Success"] = "Review deleted successfully.";

            return RedirectToAction(nameof(Reviews));
        }

        [HttpGet]
        public async Task<IActionResult> Notifications()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Account");

            var notifications = await _context.Notifications
                .AsNoTracking()
                .Where(n => n.UserId == user.Id)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            ViewBag.UnreadCount = notifications.Count(n => !n.IsRead);

            return View(notifications);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkNotificationAsRead(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Account");

            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n =>
                    n.NotificationId == id &&
                    n.UserId == user.Id);

            if (notification == null)
                return NotFound();

            notification.IsRead = true;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Notifications));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAllNotificationsAsRead()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Account");

            var notifications = await _context.Notifications
                .Where(n =>
                    n.UserId == user.Id &&
                    !n.IsRead)
                .ToListAsync();

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "All notifications marked as read.";

            return RedirectToAction(nameof(Notifications));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteNotification(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Account");

            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n =>
                    n.NotificationId == id &&
                    n.UserId == user.Id);

            if (notification == null)
                return NotFound();

            _context.Notifications.Remove(notification);

            await _context.SaveChangesAsync();

            TempData["Success"] = "Notification deleted successfully.";

            return RedirectToAction(nameof(Notifications));
        }

        private string BuildVendorOrderEmail(
    string customerName,
    string customerEmail,
    string orderNumber,
    List<CartItem> items,
    PickupSlot? pickupSlot)
        {
            var productRows = "";

            foreach (var item in items)
            {
                var productName =
                    item.Product?.ProductName ?? "Product";

                var quantity =
                    item.Quantity;

                var unitPrice =
                    item.UnitPrice.ToString("N2");

                var totalPrice =
                    item.TotalPrice.ToString("N2");

                productRows += $@"
            <tr>
                <td style='padding:10px; border-bottom:1px solid #ddd;'>
                    {productName}
                </td>

                <td style='padding:10px; border-bottom:1px solid #ddd; text-align:center;'>
                    {quantity}
                </td>

                <td style='padding:10px; border-bottom:1px solid #ddd; text-align:right;'>
                    Rs. {unitPrice}
                </td>

                <td style='padding:10px; border-bottom:1px solid #ddd; text-align:right;'>
                    Rs. {totalPrice}
                </td>
            </tr>";
            }

            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
</head>

<body style='margin:0; padding:0; background:#f5f7f5; font-family:Arial, sans-serif;'>

    <div style='max-width:700px; margin:30px auto; background:white; border-radius:12px; overflow:hidden; box-shadow:0 4px 15px rgba(0,0,0,0.08);'>

        <!-- Header -->
        <div style='background:#1f4d3a; padding:25px; text-align:center;'>
            <h1 style='color:white; margin:0;'>
                MarketLink
            </h1>

            <p style='color:#e8f3ec; margin:8px 0 0;'>
                New Order Received
            </p>
        </div>


        <!-- Content -->
        <div style='padding:30px;'>

            <h2 style='color:#1f4d3a;'>
                New Order Received
            </h2>

            <p>
                Hello Vendor,
            </p>

            <p>
                You have received a new order through MarketLink.
            </p>


            <!-- Order Information -->
            <div style='background:#f3f7f4; padding:15px; border-radius:8px; margin:20px 0;'>

                <p style='margin:5px 0;'>
                    <strong>Order Number:</strong>
                    {orderNumber}
                </p>

                <p style='margin:5px 0;'>
                    <strong>Customer:</strong>
                    {customerName}
                </p>

                <p style='margin:5px 0;'>
                    <strong>Customer Email:</strong>
                    {customerEmail}
                </p>

            </div>


            <!-- Products -->
            <h3 style='color:#1f4d3a;'>
                Ordered Products
            </h3>

            <table style='width:100%; border-collapse:collapse;'>

                <thead>

                    <tr style='background:#1f4d3a; color:white;'>

                        <th style='padding:10px; text-align:left;'>
                            Product
                        </th>

                        <th style='padding:10px;'>
                            Quantity
                        </th>

                        <th style='padding:10px; text-align:right;'>
                            Unit Price
                        </th>

                        <th style='padding:10px; text-align:right;'>
                            Total
                        </th>

                    </tr>

                </thead>

                <tbody>

                    {productRows}

                </tbody>

            </table>


            <div style='margin-top:25px; padding:15px; background:#eef6f0; border-radius:8px;'>

                <p style='font-size:18px; margin:0;'>

                    <strong>
                        Order Total:
                    </strong>

                    Rs. {items.Sum(x => x.TotalPrice):N2}

                </p>

            </div>


            <p style='margin-top:25px;'>
                Please login to your MarketLink vendor account
                to manage this order.
            </p>

        </div>


        <!-- Footer -->
        <div style='background:#1f4d3a; padding:18px; text-align:center;'>

            <p style='color:white; margin:0;'>
                © MarketLink
            </p>

        </div>

    </div>

</body>
</html>";
        }

        private string BuildCustomerOrderEmail(
    string customerName,
    string orderNumber,
    decimal totalAmount,
    List<CartItem> items,
    PickupSlot? pickupSlot)
        {
            var productRows = "";

            foreach (var item in items)
            {
                var productName = item.Product?.ProductName ?? "Product";

                productRows += $@"
        <tr>
            <td style='padding:12px; border-bottom:1px solid #ddd;'>
                {productName}
            </td>

            <td style='padding:12px; border-bottom:1px solid #ddd; text-align:center;'>
                {item.Quantity}
            </td>

            <td style='padding:12px; border-bottom:1px solid #ddd; text-align:right;'>
                Rs. {item.UnitPrice:N2}
            </td>

            <td style='padding:12px; border-bottom:1px solid #ddd; text-align:right;'>
                Rs. {item.TotalPrice:N2}
            </td>
        </tr>";
            }

            var pickupText = pickupSlot != null
                ? "Your selected pickup slot has been recorded."
                : "No pickup slot was selected.";

            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <title>MarketLink Order Confirmation</title>
</head>

<body style='margin:0; padding:0; background:#f4f7f5; font-family:Arial, sans-serif;'>

    <div style='max-width:700px; margin:30px auto; background:#ffffff; border-radius:12px; overflow:hidden;'>

        <!-- Header -->
        <div style='background:#1f4d3a; padding:30px; text-align:center;'>
            <h1 style='color:white; margin:0;'>
                MarketLink
            </h1>

            <p style='color:#e5eee9; margin:8px 0 0;'>
                Order Confirmation
            </p>
        </div>

        <!-- Content -->
        <div style='padding:30px;'>

            <h2 style='color:#1f4d3a;'>
                Thank You, {customerName}!
            </h2>

            <p>
                Your order has been successfully placed on MarketLink.
            </p>

            <!-- Order Information -->
            <div style='background:#f1f6f3; padding:18px; border-radius:8px; margin:20px 0;'>

                <p style='margin:6px 0;'>
                    <strong>Order Number:</strong>
                    {orderNumber}
                </p>

                <p style='margin:6px 0;'>
                    <strong>Order Status:</strong>
                    Pending
                </p>

                <p style='margin:6px 0;'>
                    <strong>Pickup:</strong>
                    {pickupText}
                </p>

            </div>

            <!-- Products -->
            <h3 style='color:#1f4d3a;'>
                Your Order
            </h3>

            <table style='width:100%; border-collapse:collapse;'>

                <thead>
                    <tr style='background:#1f4d3a; color:white;'>

                        <th style='padding:12px; text-align:left;'>
                            Product
                        </th>

                        <th style='padding:12px; text-align:center;'>
                            Quantity
                        </th>

                        <th style='padding:12px; text-align:right;'>
                            Price
                        </th>

                        <th style='padding:12px; text-align:right;'>
                            Total
                        </th>

                    </tr>
                </thead>

                <tbody>
                    {productRows}
                </tbody>

            </table>

            <!-- Total -->
            <div style='margin-top:25px; padding:18px; background:#eaf3ed; border-radius:8px; text-align:right;'>

                <span style='font-size:18px;'>
                    <strong>Total Amount:</strong>
                </span>

                <span style='font-size:20px; color:#1f4d3a;'>
                    <strong>
                        Rs. {totalAmount:N2}
                    </strong>
                </span>

            </div>

            <p style='margin-top:25px;'>
                We will keep you updated about your order.
            </p>

            <p>
                Thank you for shopping with MarketLink!
            </p>

        </div>

        <!-- Footer -->
        <div style='background:#1f4d3a; padding:20px; text-align:center;'>

            <p style='color:white; margin:0;'>
                © MarketLink
            </p>

        </div>

    </div>

</body>
</html>";
        }

        private string BuildAdminOrderEmail(
    string customerName,
    string? customerEmail,
    string orderNumber,
    decimal totalAmount,
    List<CartItem> items,
    PickupSlot? pickupSlot)
        {
            var productRows = "";

            foreach (var item in items)
            {
                var productName =
                    item.Product?.ProductName ?? "Product";

                productRows += $@"
        <tr>
            <td style='padding:12px; border-bottom:1px solid #ddd;'>
                {productName}
            </td>

            <td style='padding:12px; border-bottom:1px solid #ddd; text-align:center;'>
                {item.Quantity}
            </td>

            <td style='padding:12px; border-bottom:1px solid #ddd; text-align:right;'>
                Rs. {item.UnitPrice:N2}
            </td>

            <td style='padding:12px; border-bottom:1px solid #ddd; text-align:right;'>
                Rs. {item.TotalPrice:N2}
            </td>
        </tr>";
            }

            var pickupText = pickupSlot != null
                ? "Customer selected a pickup slot."
                : "No pickup slot selected.";

            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <title>New MarketLink Order</title>
</head>

<body style='margin:0; padding:0; background:#f4f7f5; font-family:Arial, sans-serif;'>

    <div style='max-width:750px; margin:30px auto; background:#ffffff; border-radius:12px; overflow:hidden;'>

        <!-- Header -->
        <div style='background:#1f4d3a; padding:30px; text-align:center;'>

            <h1 style='color:white; margin:0;'>
                MarketLink
            </h1>

            <p style='color:#e5eee9; margin:8px 0 0;'>
                New Order Notification
            </p>

        </div>


        <!-- Content -->
        <div style='padding:30px;'>

            <h2 style='color:#1f4d3a;'>
                New Order Received
            </h2>

            <p>
                A new customer order has been placed on MarketLink.
            </p>


            <!-- Customer Information -->
            <div style='background:#f1f6f3; padding:18px; border-radius:8px; margin:20px 0;'>

                <h3 style='color:#1f4d3a; margin-top:0;'>
                    Customer Information
                </h3>

                <p style='margin:6px 0;'>
                    <strong>Customer Name:</strong>
                    {customerName}
                </p>

                <p style='margin:6px 0;'>
                    <strong>Customer Email:</strong>
                    {customerEmail ?? "Not available"}
                </p>

                <p style='margin:6px 0;'>
                    <strong>Order Number:</strong>
                    {orderNumber}
                </p>

                <p style='margin:6px 0;'>
                    <strong>Order Status:</strong>
                    Pending
                </p>

                <p style='margin:6px 0;'>
                    <strong>Pickup:</strong>
                    {pickupText}
                </p>

            </div>


            <!-- Products -->
            <h3 style='color:#1f4d3a;'>
                Ordered Products
            </h3>

            <table style='width:100%; border-collapse:collapse;'>

                <thead>

                    <tr style='background:#1f4d3a; color:white;'>

                        <th style='padding:12px; text-align:left;'>
                            Product
                        </th>

                        <th style='padding:12px; text-align:center;'>
                            Quantity
                        </th>

                        <th style='padding:12px; text-align:right;'>
                            Unit Price
                        </th>

                        <th style='padding:12px; text-align:right;'>
                            Total
                        </th>

                    </tr>

                </thead>

                <tbody>

                    {productRows}

                </tbody>

            </table>


            <!-- Total -->
            <div style='margin-top:25px; padding:18px; background:#eaf3ed; border-radius:8px; text-align:right;'>

                <span style='font-size:18px;'>
                    <strong>
                        Total Order Amount:
                    </strong>
                </span>

                <span style='font-size:20px; color:#1f4d3a;'>
                    <strong>
                        Rs. {totalAmount:N2}
                    </strong>
                </span>

            </div>


            <p style='margin-top:25px;'>
                Please login to the MarketLink Admin Panel
                to manage this order.
            </p>

        </div>


        <!-- Footer -->
        <div style='background:#1f4d3a; padding:20px; text-align:center;'>

            <p style='color:white; margin:0;'>
                © MarketLink
            </p>

        </div>

    </div>

</body>
</html>";
        }
    }
}