﻿namespace MentalHealthApis.Models
{
    public enum AppointmentStatus
    {
        Pending,
        Confirmed, // Added for clarity, doctor might confirm
        Completed,
        CancelledByAdmin,
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
    public enum PostStatus
    {
        Draft = 0,
        Published = 1,
        Archived = 2,
        Rejected = 3
    }
}