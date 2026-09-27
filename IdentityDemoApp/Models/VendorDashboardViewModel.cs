using IdentityDemoApp.Models;

namespace IdentityDemoApp.Models.ViewModels
{
    public class VendorDashboardViewModel
    {
        public int VendorProfileId { get; set; }

        public string FarmName { get; set; } = string.Empty;

        public string City { get; set; } = string.Empty;

        public string Province { get; set; } = string.Empty;

        public string FarmDescription { get; set; } = string.Empty;

        public bool IsApproved { get; set; }


      
        public int TotalProducts { get; set; }

        public int AvailableProducts { get; set; }

        public int PendingProducts { get; set; }

        public int ApprovedProducts { get; set; }


        public int TotalWeeklyStock { get; set; }


 
        public int TotalPickupSlots { get; set; }

        public int AvailablePickupSlots { get; set; }


      
        public int TotalOrders { get; set; }

        public int PendingOrders { get; set; }

        public int CompletedOrders { get; set; }


     
        public decimal TotalSales { get; set; }


     
        public int TotalReviews { get; set; }

        public double AverageRating { get; set; }


        public List<PickupSlot> UpcomingPickupSlots { get; set; }
            = new List<PickupSlot>();
    }
}