using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Turnos.Services;

namespace Turnos.Controllers;

[Route("events")]
[Authorize(Policy = "AppAccess")]
public class EventDocumentsController : Controller
{
    private readonly EventContractService _contracts;

    public EventDocumentsController(EventContractService contracts) => _contracts = contracts;

    [HttpGet("{id:int}/contract")]
    public async Task<IActionResult> GetContract(int id)
    {
        var contract = await _contracts.GetDataAsync(id);
        if (contract is null) return NotFound();

        return File(contract.Value.Data, contract.Value.ContentType, contract.Value.FileName);
    }
}
