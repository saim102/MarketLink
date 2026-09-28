using IdentityDemoApp.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IdentityDemoApp.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        

        public DbSet<VendorProfile> VendorProfiles { get; set; }
        public DbSet<CustomerProfile> CustomerProfiles { get; set; }
        public DbSet<Market> Markets { get; set; }
        public DbSet<VendorMarket> VendorMarkets { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<WeeklyStock> WeeklyStocks { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<PickupSlot> PickupSlots { get; set; }
        public DbSet<FavouriteVendor> FavouriteVendors { get; set; }
        public DbSet<FavouriteProduct> FavouriteProducts { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<ReviewResponse> ReviewResponses { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<FavouriteMarket> FavouriteMarkets { get; set; }




        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);




            builder.Entity<VendorMarket>()
                .HasOne(vm => vm.VendorProfile)
                .WithMany(v => v.VendorMarkets)
                .HasForeignKey(vm => vm.VendorProfileId)
                .OnDelete(DeleteBehavior.Cascade);


            builder.Entity<VendorMarket>()
                .HasOne(vm => vm.Market)
                .WithMany(m => m.VendorMarkets)
                .HasForeignKey(vm => vm.MarketId)
                .OnDelete(DeleteBehavior.Cascade);




            builder.Entity<Product>()
                .HasOne(p => p.VendorProfile)
                .WithMany(v => v.Products)
                .HasForeignKey(p => p.VendorProfileId)
                .OnDelete(DeleteBehavior.Restrict);


            builder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);




            builder.Entity<WeeklyStock>()
                .HasOne(w => w.Product)
                .WithMany(p => p.WeeklyStocks)
                .HasForeignKey(w => w.ProductId)
                .OnDelete(DeleteBehavior.Cascade);


            builder.Entity<WeeklyStock>()
                .HasOne(w => w.VendorProfile)
                .WithMany(v => v.WeeklyStocks)
                .HasForeignKey(w => w.VendorProfileId)
                .OnDelete(DeleteBehavior.Restrict);



            builder.Entity<PickupSlot>()
                .HasOne(p => p.VendorProfile)
                .WithMany(v => v.PickupSlots)
                .HasForeignKey(p => p.VendorProfileId)
                .OnDelete(DeleteBehavior.Cascade);



            builder.Entity<Order>()
                .HasOne(o => o.CustomerProfile)
                .WithMany(c => c.Orders)
                .HasForeignKey(o => o.CustomerProfileId)
                .OnDelete(DeleteBehavior.Restrict);


            builder.Entity<Order>()
                .HasOne(o => o.PickupSlot)
                .WithMany(p => p.Orders)
                .HasForeignKey(o => o.PickupSlotId)
                .OnDelete(DeleteBehavior.SetNull);




            builder.Entity<OrderItem>()
                .HasOne(i => i.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(i => i.OrderId)
                .OnDelete(DeleteBehavior.Cascade);


            builder.Entity<OrderItem>()
                .HasOne(i => i.Product)
                .WithMany(p => p.OrderItems)
                .HasForeignKey(i => i.ProductId)
                .OnDelete(DeleteBehavior.Restrict);




            builder.Entity<Cart>()
                .HasOne(c => c.CustomerProfile)
                .WithOne(c => c.Cart)
                .HasForeignKey<Cart>(c => c.CustomerProfileId)
                .OnDelete(DeleteBehavior.Cascade);


            builder.Entity<CartItem>()
                .HasOne(i => i.Cart)
                .WithMany(c => c.CartItems)
                .HasForeignKey(i => i.CartId)
                .OnDelete(DeleteBehavior.Cascade);


            builder.Entity<CartItem>()
                .HasOne(i => i.Product)
                .WithMany(p => p.CartItems)
                .HasForeignKey(i => i.ProductId)
                .OnDelete(DeleteBehavior.Restrict);




            builder.Entity<FavouriteVendor>()
                .HasOne(f => f.CustomerProfile)
                .WithMany(c => c.FavouriteVendors)
                .HasForeignKey(f => f.CustomerProfileId)
                .OnDelete(DeleteBehavior.Cascade);


            builder.Entity<FavouriteVendor>()
                .HasOne(f => f.VendorProfile)
                .WithMany(v => v.FavouriteVendors)
                .HasForeignKey(f => f.VendorProfileId)
                .OnDelete(DeleteBehavior.Restrict);



            builder.Entity<FavouriteProduct>()
                .HasOne(f => f.CustomerProfile)
                .WithMany(c => c.FavouriteProducts)
                .HasForeignKey(f => f.CustomerProfileId)
                .OnDelete(DeleteBehavior.Cascade);


            builder.Entity<FavouriteProduct>()
                .HasOne(f => f.Product)
                .WithMany(p => p.FavouriteProducts)
                .HasForeignKey(f => f.ProductId)
                .OnDelete(DeleteBehavior.Restrict);




            builder.Entity<Review>()
                .HasOne(r => r.CustomerProfile)
                .WithMany(c => c.Reviews)
                .HasForeignKey(r => r.CustomerProfileId)
                .OnDelete(DeleteBehavior.Restrict);


            builder.Entity<Review>()
                .HasOne(r => r.Product)
                .WithMany(p => p.Reviews)
                .HasForeignKey(r => r.ProductId)
                .OnDelete(DeleteBehavior.Cascade);



            builder.Entity<ReviewResponse>()
                .HasOne(r => r.Review)
                .WithMany(r => r.ReviewResponses)
                .HasForeignKey(r => r.ReviewId)
                .OnDelete(DeleteBehavior.Cascade);




            builder.Entity<VendorProfile>()
                .HasIndex(v => v.UserId)
                .IsUnique();



            builder.Entity<CustomerProfile>()
                .HasIndex(c => c.UserId)
                .IsUnique();



            builder.Entity<Cart>()
                .HasIndex(c => c.CustomerProfileId)
                .IsUnique();




            builder.Entity<FavouriteVendor>()
                .HasIndex(f => new
                {
                    f.CustomerProfileId,
                    f.VendorProfileId
                })
                .IsUnique();




            builder.Entity<FavouriteProduct>()
                .HasIndex(f => new
                {
                    f.CustomerProfileId,
                    f.ProductId
                })
                .IsUnique();




            builder.Entity<WeeklyStock>()
                .HasIndex(w => new
                {
                    w.ProductId,
                    w.WeekStartDate
                })
                .IsUnique();



            builder.Entity<Order>()
                .HasIndex(o => o.OrderNumber)
                .IsUnique();

          
            builder.Entity<FavouriteMarket>()
                .HasOne(f => f.CustomerProfile)
                .WithMany()
                .HasForeignKey(f => f.CustomerProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<FavouriteMarket>()
                .HasOne(f => f.Market)
                .WithMany()
                .HasForeignKey(f => f.MarketId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<FavouriteMarket>()
                .HasIndex(f => new
                {
                    f.CustomerProfileId,
                    f.MarketId
                })
                .IsUnique();


        }
            
        }
}