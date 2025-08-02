using MentalHealthApis.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class DoctorDocumentsController : ControllerBase
{
    private readonly IDoctorDocumentService _documentService;

    public DoctorDocumentsController(IDoctorDocumentService documentService)
    {
        _documentService = documentService;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadDocuments([FromForm] DoctorDocumentUploadDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var success = await _documentService.ProcessAndSaveDocumentsAsync(dto);

        if (success)
        {
            return Ok(new { message = "Documents submitted successfully and are now under review." });
        }

        return StatusCode(500, new { message = "An error occurred while processing your documents." });
    }
}