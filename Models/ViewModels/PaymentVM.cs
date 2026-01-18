namespace TourismManagementSystem.Models.ViewModels
{
    public class PaymentVM
    {
        // Booking Info
        public int PackageId { get; set; }
        public string PackageType { get; set; }
        public double TotalAmount { get; set; }

        // User Info
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }

        // Destination Info
        public string DestinationName { get; set; }

        // Payment Details (will be filled by user)
        public string PaymentMethod { get; set; }

        // Card Details (if Credit/Debit selected)
        public string CardHolderName { get; set; }
        public string CardNumber { get; set; }
        public string ExpiryDate { get; set; }
        public string CVV { get; set; }

        // Mobile Wallet Details (if JazzCash/Easypaisa)
        public string MobileNumber { get; set; }
        public string WalletPin { get; set; }

        // Bank Transfer Details
        public string BankName { get; set; }
        public string AccountNumber { get; set; }
        public string TransactionReference { get; set; }
    }
}