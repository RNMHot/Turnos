using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Turnos.Services;

namespace Turnos.Controllers;

[Route("companies")]
[Authorize(Policy = "AppAccess")]
public class CompanyDocumentsController : Controller
{
    private readonly CompanyDocumentService _documents;

    public CompanyDocumentsController(CompanyDocumentService documents) => _documents = documents;

    [HttpGet("{id:int}/document")]
    public async Task<IActionResult> GetDocument(int id)
    {
        var document = await _documents.GetDataAsync(id);
        if (document is null) return NotFound();

        return File(document.Value.Data, document.Value.ContentType, document.Value.FileName);
    }
}
