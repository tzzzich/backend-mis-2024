using Backend_aspnet_lab.dto.PatientController;
using Backend_aspnet_lab.dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Backend_aspnet_lab.dto.InspectionController;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Backend_aspnet_lab.Models;
using Microsoft.EntityFrameworkCore;
using Backend_aspnet_lab.Utils;
using Backend_aspnet_lab.Utils.Exceptions;
using Backend_aspnet_lab.Utils.Service;

namespace Backend_aspnet_lab.Controllers
{
    [Route("api/inspection")]
    [Produces("application/json")]
    [ApiController]
    [Authorize]
    public class InspectionController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly InspectionService _inspectionService;

        public InspectionController(ApplicationDbContext context)
        {
            _context = context;
            _inspectionService = new InspectionService();
        }

        [HttpGet("{id}")]
        [ProducesResponseType(200, Type = typeof(InspectionModel))]
        [ProducesResponseType(500, Type = typeof(ResponseModel))]
        public async Task<IActionResult> GetInspectionInfo(Guid id)
        {
            try
            {
                var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                if (!User.Identity.IsAuthenticated || TokenBlackList.TokenBlacklisted(token))
                {
                    throw new UnauthorizedAccessException("Unauthorized");
                }

                var doctorIdParsed = Guid.TryParse(User.FindFirstValue(JwtRegisteredClaimNames.Jti), out var doctorGuid);

                var inspection = await _context.Inspections
                    .Include(i => i.PreviousInspection)
                    .Include(i => i.Diagnoses)
                        .ThenInclude(r => r.Icd10Record)
                    .Include(i => i.Consultations)
                        .ThenInclude(s => s.Speciality)
                     .Include(i => i.Consultations)
                        .ThenInclude(c => c.Comments)
                    .Include(i => i.Doctor)
                    .Include(i => i.Patient)
                    .Where(i => i.Id == id)
                    .FirstOrDefaultAsync();

                if (inspection == null)
                {
                    throw new KeyNotFoundException($"Inspection with id:{id} was not found");
                }

                var result = new InspectionModel(inspection);

                result.BaseInspectionId = await _inspectionService.GetBaseInspectionId(inspection.Id, _context);

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new ResponseModel { Status = "Error", Message = $"{ex.Message}" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ResponseModel { Status = "Error", Message = $"Patient with id:{id} was not found" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseModel { Status = "InternalServerError", Message = $"{ex.Message}" });
            }

        }


        [HttpPut("{id}")]
        [ProducesResponseType(500, Type = typeof(ResponseModel))]
        public async Task<IActionResult> GetInspectionInfo(
            Guid id,
            [FromBody] InspectionEditModel model)
        {
            try
            {
                var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                if (!User.Identity.IsAuthenticated || TokenBlackList.TokenBlacklisted(token))
                {
                    throw new UnauthorizedAccessException("Unauthorized");
                }

                var doctorIdParsed = Guid.TryParse(User.FindFirstValue(JwtRegisteredClaimNames.Jti), out var doctorGuid);

                var inspection = await _context.Inspections
                    .Include(i => i.Diagnoses)
                        .ThenInclude(r => r.Icd10Record)
                    .Where(i => i.Id == id)
                    .FirstOrDefaultAsync();

                if (inspection == null)
                {
                    throw new KeyNotFoundException($"Inspection with id:{id} was not found");
                }
                
                if (inspection.DoctorId != doctorGuid)
                {
                    throw new ForbiddenAccessException($"Doctor with id {doctorGuid} does not have editing rights for this inspection");
                }

                if (!ModelState.IsValid) { return BadRequest(ModelState); }

                inspection.Anamnesis = model.Anamnesis;
                inspection.Complaints = model.Complaints;
                inspection.Treatment = model.Treatment;
                inspection.Conclusion= model.Conclusion;
                inspection.NextVisitDate = model.NextVisitDate;
                inspection.DeathDate = model.DeathDate;
                inspection.Diagnoses = model.Diagnoses.Select(d => new Diagnosis
                {
                    Icd10RecordId = d.IcdDiagnosisId,
                    Description = d.Description,
                    Type = d.Type,

                }).ToList() ;

                await _context.SaveChangesAsync();

                return Ok();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new ResponseModel { Status = "Error", Message = $"{ex.Message}" });
            }
            catch (ForbiddenAccessException ex)
            {
                return StatusCode(403, new ResponseModel { Status = "Forbidden", Message = $"{ex.Message}" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ResponseModel { Status = "Error", Message = $"Patient with id:{id} was not found" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseModel { Status = "InternalServerError", Message = $"{ex.Message}" });
            }

        }

        [HttpGet("{id}/chain")]
        [ProducesResponseType(200, Type = typeof(List<InspectionPreviewModel>))]
        [ProducesResponseType(500, Type = typeof(ResponseModel))]
        public async Task<IActionResult> PutInspectionInfo(
            Guid id
            )
        {
            try
            {
                var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                if (!User.Identity.IsAuthenticated || TokenBlackList.TokenBlacklisted(token))
                {
                    throw new UnauthorizedAccessException("Unauthorized");
                }

                var doctorIdParsed = Guid.TryParse(User.FindFirstValue(JwtRegisteredClaimNames.Jti), out var doctorGuid);

                var inspection = await _context.Inspections
                    .Include(i => i.Diagnoses)
                        .ThenInclude(r => r.Icd10Record)
                    .Include(i => i.Consultations)
                        .ThenInclude(s => s.Speciality)
                     .Include(i => i.Consultations)
                        .ThenInclude(c => c.Comments)
                    .Include(i => i.ChildInspection)
                    .Include(i => i.Doctor)
                    .Include(i => i.Patient)
                    .Where(i => i.Id == id)
                    .FirstOrDefaultAsync();

                if (inspection == null)
                {
                    throw new KeyNotFoundException($"Inspection with Id {id} was not found");
                }

                if (inspection.PreviousInspectionId != null)
                {
                    throw new ArgumentException($"Inspection {inspection.Id} is not a root inspection");
                }



                var result = new List<InspectionPreviewModel>();

                while (inspection.ChildInspection != null)
                {
                    var childId = inspection.ChildInspection.Id;
                    inspection = await _context.Inspections
                        .Include(i => i.Diagnoses)
                            .ThenInclude(r => r.Icd10Record)
                        .Include(i => i.Consultations)
                            .ThenInclude(s => s.Speciality)
                         .Include(i => i.Consultations)
                            .ThenInclude(c => c.Comments)
                        .Include(i => i.ChildInspection)
                        .Include(i => i.Doctor)
                        .Include(i => i.Patient)
                        .Where(i => i.Id == childId)
                        .FirstOrDefaultAsync();

                    if (inspection != null)
                    {
                        result.Add(new InspectionPreviewModel(inspection));
                    } 
                }

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ResponseModel { Status = "Error", Message = $"{ex.Message}" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new ResponseModel { Status = "Error", Message = $"{ex.Message}" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ResponseModel { Status = "Error", Message = $"Patient with id:{id} was not found" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseModel { Status = "InternalServerError", Message = $"{ex.Message}" });
            }

        }
    }
}
