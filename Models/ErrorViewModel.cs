namespace TourismManagementSystem.Models
{
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }  // ? Nullable fixed

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
