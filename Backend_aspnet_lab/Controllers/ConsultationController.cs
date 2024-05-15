using Backend_aspnet_lab.dto.PatientController;
using Backend_aspnet_lab.dto;
using Backend_aspnet_lab.Models;
using Backend_aspnet_lab.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Identity;
using Backend_aspnet_lab.dto.ConsultationController;
using Backend_aspnet_lab.Utils.Exceptions;
using Backend_aspnet_lab.Utils.Service;

namespace Backend_aspnet_lab.Controllers
{
    [Route("api/consultation")]
    [Produces("application/json")]
    [ApiController]
    [Authorize]
    public class ConsultationController : ControllerBase
    {

        private readonly ApplicationDbContext _context;
        private readonly UserManager<Doctor> _userManager;

        public ConsultationController(UserManager<Doctor> userManager, ApplicationDbContext context)
        {
            _context = context;
            _userManager = userManager;
        }


        [HttpGet]
        [ProducesResponseType(200, Type = typeof(InspectionPagedListModel))]
        [ProducesResponseType(500, Type = typeof(ResponseModel))]
        public async Task<IActionResult> GetConsultationInspections(
            [FromQuery] List<Guid>? icdRoots = null,
            [FromQuery] bool? grouped = false,
            [Range(1, Int32.MaxValue)] int page = 1,
            [Range(1, Int32.MaxValue)] int size = 5)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                if (!User.Identity.IsAuthenticated || TokenBlackList.TokenBlacklisted(token))
                {
                    throw new UnauthorizedAccessException("Unauthorized");
                }

                var doctorIdParsed = Guid.TryParse(User.FindFirstValue(JwtRegisteredClaimNames.Jti), out var doctorGuid);

                Doctor? doctor = await _userManager.FindByIdAsync(User.FindFirstValue(JwtRegisteredClaimNames.Jti));

                if (doctor == null)
                {
                    throw new KeyNotFoundException($"Doctor with id:{doctorGuid} was not found");
                }

                var filteredInspections = _context.Inspections
                    .Include(i => i.PreviousInspection)
                    .Include(i => i.Diagnoses)
                        .ThenInclude(r => r.Icd10Record)
                    .Include(i => i.Consultations)
                    .Include(i => i.Doctor)
                    .Include(i => i.Patient)
                    .Where(i => i.Consultations.Any(c => c.SpecialityId == doctor.SpecialityId))
                    .ToList()
                    .OrderBy(i => i.CreateTime);

                if (grouped == true)
                {
                    filteredInspections = filteredInspections
                        .Where(x => x.PreviousInspectionId == null)
                        .OrderBy(x => x.CreateTime);
                }

                var icdRootsDb = _context.Icd10Records
                   .Where(s => s.PreviousId == null)
                   .ToList()
                   .OrderBy(s => s.Code);

                if (icdRoots.Count > 0)
                {
                    foreach (var root in icdRoots)
                    {
                        if (icdRootsDb.FirstOrDefault(x => x.Id == root) == null)
                        {
                            throw new ArgumentException($"{root} is not a root Icd10");
                        }
                    }
                }

                List<Inspection> filteredInspectionsIcd = new List<Inspection>();

                foreach (var inspection in filteredInspections)
                {
                    if (icdRoots.Count == 0
                        || icdRoots.Count > 0 && inspection.Diagnoses.
                        Any(d => d.Type == DiagnosisType.Main && icdRoots.Contains(IcdService.FindIcdRoot(d.Icd10Record)))
                        )
                    {
                        filteredInspectionsIcd.Add(inspection);
                    }
                }

                InspectionPagedListModel result = new InspectionPagedListModel();
                result.Inspections = new List<InspectionPreviewModel>();
  
                foreach (var inspection in filteredInspectionsIcd.Skip((page - 1) * size).Take(size))
                {
                    result.Inspections.Add(new InspectionPreviewModel(inspection));
                }

                result.Pagination = Pagination.MakePagination(size, page, result.Inspections.Count());

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
                return NotFound(new ResponseModel { Status = "Error", Message = $"{ex.Message}" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseModel { Status = "InternalServerError", Message = $"{ex.Message}" });
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(200, Type = typeof(ConsultationModel))]
        [ProducesResponseType(500, Type = typeof(ResponseModel))]
        public async Task<IActionResult> GetConsultation(
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

                Consultation? consultation = await _context.Consultations
                    .Include(x => x.Speciality)
                    .Include(x => x.Inspection)
                    .Include(x => x.Comments)
                        .ThenInclude(c => c.Author)
                    .FirstOrDefaultAsync(x => x.Id == id);


                if (consultation == null)
                {
                    throw new KeyNotFoundException($"Consultation with id:{id} was not found");
                }

                var result = new ConsultationModel(consultation);

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new ResponseModel { Status = "Error", Message = $"{ex.Message}" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ResponseModel { Status = "Error", Message = $"{ex.Message}" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseModel { Status = "InternalServerError", Message = $"{ex.Message}" });
            }
        }

        [HttpPost("{id}/comment")]
        [ProducesResponseType(200, Type = typeof(Guid))]
        [ProducesResponseType(500, Type = typeof(ResponseModel))]
        public async Task<IActionResult> PostComment(
            Guid id,
            [FromBody] CommentCreateModel model
            )
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

                if (!User.Identity.IsAuthenticated || TokenBlackList.TokenBlacklisted(token))
                {
                    throw new UnauthorizedAccessException("Unauthorized");
                }

                var doctorIdParsed = Guid.TryParse(User.FindFirstValue(JwtRegisteredClaimNames.Jti), out var doctorGuid);
                
                var doctor = await _userManager.FindByIdAsync(doctorGuid.ToString());

                Consultation? consultation = await _context.Consultations
                    .Include(x => x.Speciality)
                    .Include(x => x.Inspection)
                        .ThenInclude(i => i.Doctor)
                    .Include(x => x.Inspection)
                    .Include(x => x.Comments)
                    .FirstOrDefaultAsync(x => x.Id == id);


                if (consultation == null)
                {
                    throw new KeyNotFoundException($"Consultation with id:{id} was not found");
                }

                if (consultation.SpecialityId != doctor.SpecialityId && doctor.Id != consultation.Inspection.Doctor.Id)
                {
                    throw new ForbiddenAccessException($"Doctor with id: {doctorGuid} does not have commenting rights for this consultation");
                }

                if (model.ParentId != null)
                {
                    var parentComment = await _context.Comments
                    .FirstOrDefaultAsync(x => x.Id == model.ParentId);

                    if (parentComment == null)
                    {
                        throw new KeyNotFoundException($"Comment with id :{model.ParentId} was not found");
                    }

                    if (parentComment.ConsultationId != consultation.Id)
                    {
                        throw new ArgumentException($"Parent comment with id: {parentComment.Id} is not associated with the consultation with id: {id}");
                    }
                }

                Comment comment = new Comment
                {
                    Content = model.Content,
                    AuthorId = doctorGuid,
                    ParentId = model.ParentId,
                    ConsultationId = consultation.Id

                };

                consultation.Comments.Add(comment);

                _context.Comments.Add(comment);

                await _context.SaveChangesAsync();

                return Ok(comment.Id);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ResponseModel { Status = "Error", Message = $"{ex.Message}" });
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
                return NotFound(new ResponseModel { Status = "Error", Message = $"{ex.Message}" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseModel { Status = "InternalServerError", Message = $"{ex.Message}" });
            }
        }


        [HttpPut("comment/{id}")]
        [ProducesResponseType(200, Type = typeof(Guid))]
        [ProducesResponseType(500, Type = typeof(ResponseModel))]
        public async Task<IActionResult> PutComment(
            Guid id,
            [FromBody] InspectionCommentCreateModel model
            )
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

                if (!User.Identity.IsAuthenticated || TokenBlackList.TokenBlacklisted(token))
                {
                    throw new UnauthorizedAccessException("Unauthorized");
                }

                var doctorIdParsed = Guid.TryParse(User.FindFirstValue(JwtRegisteredClaimNames.Jti), out var doctorGuid);

                var doctor = await _userManager.FindByIdAsync(doctorGuid.ToString());

                Comment? comment = await _context.Comments
                    .Include(x => x.Author)
                    .Include(x => x.Consultation)
                    .Include(x => x.Parent)
                    .FirstOrDefaultAsync(x => x.Id == id);


                if (comment == null)
                {
                    throw new KeyNotFoundException($"Consultation with id:{id} was not found");
                }

                if (doctor.Id != comment.Author.Id)
                {
                    throw new ForbiddenAccessException($"Doctor with id: {doctorGuid} does not have editing rights");
                }

                comment.Content = model.Content;

                await _context.SaveChangesAsync();

                return Ok();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ResponseModel { Status = "Error", Message = $"{ex.Message}" });
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
                return NotFound(new ResponseModel { Status = "Error", Message = $"{ex.Message}" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseModel { Status = "InternalServerError", Message = $"{ex.Message}" });
            }
        }

    }
}
