// Services/Interfaces/IDoctorDocumentService.cs

using System.Threading.Tasks;

public interface IDoctorDocumentService
{
    Task<bool> ProcessAndSaveDocumentsAsync(DoctorDocumentUploadDto dto);
}