namespace MentalHealthApis.Models
{
    public enum AppointmentStatus
    {
        Pending,
        Confirmed, // Added for clarity, doctor might confirm
        Completed,
        CancelledByUser,
        CancelledByDoctor, // Or just "Cancelled" and track who did it
        Rescheduled
    }

    public enum UserRole
    {
        User,
        Doctor,
        Admin
    }
}