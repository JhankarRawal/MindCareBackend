namespace MentalHealthApis.DTOs
{
    public class DoctorDocumentAdminViewDto
    {
        public int Id { get; set; }
        public int DoctorId { get; set; }
        public string DoctorName { get; set; }
        public string DocumentType { get; set; }
        public string FilePath { get; set; }
        public string Status { get; set; }
    }
}