using MentalHealthApis.DTOs;
using System.Threading.Tasks;

public interface IDoctorDocumentService
{
    Task<bool> ProcessAndSaveDocumentsAsync(DoctorDocumentUploadDto dto);
}