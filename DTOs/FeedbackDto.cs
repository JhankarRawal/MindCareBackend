﻿public class FeedbackDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Comment { get; set; }
    public int Rating { get; set; }
    public DateTime SubmittedAt { get; set; }
}
