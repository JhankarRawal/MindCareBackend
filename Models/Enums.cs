namespace MentalHealthApis.Models
{
   public enum AppointmentStatus
    {
        Pending,
        Confirmed,
        Completed,
        CancelledByAdmin, // An admin initiated cancellation
        CancelledByUser,  // User initiated cancellation
        CancelledByDoctor, // Doctor initiated cancellation
        Rejected,         // Doctor explicitly declined a pending appointment
        Rescheduled
    }

    public enum UserRole
    {
        User,
        Doctor,
        Admin
    }
    public enum PostStatus
    {
        Draft = 0,
        Published = 1,
        Archived = 2,
        Rejected = 3
    }
    public enum DocumentStatus
{
Pending,
Verified,
Rejected
}
}