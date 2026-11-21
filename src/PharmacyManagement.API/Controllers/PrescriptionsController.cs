using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.DTOs.Prescriptions;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Domain.Enums;

namespace PharmacyManagement.API.Controllers;

[Route("api/prescriptions")]
public sealed class PrescriptionsController : ApiControllerBase
{
    private readonly IPrescriptionService _prescriptions;

    public PrescriptionsController(IPrescriptionService prescriptions) => _prescriptions = prescriptions;

    [HttpGet]
    [Authorize(Policy = Permissions.PrescriptionsView)]
    public async Task<ActionResult<PagedResult<PrescriptionDto>>> GetPaged(
        [FromQuery] QueryParameters query,
        [FromQuery] PrescriptionStatus? status,
        [FromQuery] int? branchId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
        => Ok(await _prescriptions.GetPagedAsync(query, status, branchId, from, to, Ct));

    [HttpGet("{id:int}")]
    [Authorize(Policy = Permissions.PrescriptionsView)]
    public async Task<ActionResult<PrescriptionDto>> GetById(int id) => Ok(await _prescriptions.GetByIdAsync(id, Ct));

    [HttpPost]
    [Authorize(Policy = Permissions.PrescriptionsCreate)]
    public async Task<ActionResult<PrescriptionDto>> Create([FromBody] CreatePrescriptionRequest request)
        => Ok(await _prescriptions.CreateAsync(request, Ct));

    [HttpPut("{id:int}")]
    [Authorize(Policy = Permissions.PrescriptionsCreate)]
    public async Task<ActionResult<PrescriptionDto>> Update(int id, [FromBody] CreatePrescriptionRequest request)
        => Ok(await _prescriptions.UpdateAsync(id, request, Ct));

    [HttpPost("{id:int}/verify")]
    [Authorize(Policy = Permissions.PrescriptionsVerify)]
    public async Task<ActionResult<PrescriptionDto>> Verify(int id) => Ok(await _prescriptions.VerifyAsync(id, Ct));

    [HttpPost("{id:int}/reject")]
    [Authorize(Policy = Permissions.PrescriptionsVerify)]
    public async Task<ActionResult<PrescriptionDto>> Reject(int id, [FromBody] RejectPrescriptionRequest request)
        => Ok(await _prescriptions.RejectAsync(id, request.Reason, Ct));

    [HttpPost("{id:int}/image")]
    [Authorize(Policy = Permissions.PrescriptionsCreate)]
    public async Task<ActionResult<PrescriptionDto>> UploadImage(int id, IFormFile file)
    {
        var (stream, fileName) = await ReadUploadAsync(file);
        await using (stream)
        {
            var path = await _prescriptions.UploadImageAsync(id, stream, fileName, Ct);
            return Ok(new { imagePath = path });
        }
    }
}
