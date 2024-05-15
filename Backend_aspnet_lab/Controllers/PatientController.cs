using Azure.Core;
using Backend_aspnet_lab.dto;
using Backend_aspnet_lab.dto.PatientController;
using Backend_aspnet_lab.Models;
using Backend_aspnet_lab.Utils;
using Backend_aspnet_lab.Utils.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.Framework;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;

namespace Backend_aspnet_lab.Controllers
{
    [Route("api/patient")]
    [Produces("application/json")]
    [ApiController]
    [Authorize]
    public class PatientController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PatientController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [ProducesResponseType(200, Type = typeof(Guid))]
        [ProducesResponseType(500, Type = typeof(ResponseModel))]
        public async Task<IActionResult> PostPatient([FromBody] PatientCreateModel model)
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

                

                var patient = new Patient
                {
                    Name = model.Name,
                    Birthday = model.Birthday,
                    Gender = model.Gender
                };

                        
                _context.Patients.Add(patient);

                _context.SaveChanges();

                return Ok(patient.Id);
                    
            }   
            catch (ArgumentException ex)
            {
                return BadRequest( new ResponseModel
                {
                    Status = "Error",
                    Message = ex.Message
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new ResponseModel
                {
                    Status = "Error",
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseModel
                {
                    Status = "InternalServerError",
                    Message = ex.Message
                });
            }
        }


        [HttpGet]
        [ProducesResponseType(200, Type = typeof(PatientPagedListModel))]
        [ProducesResponseType(500, Type = typeof(ResponseModel))]
        public async Task<IActionResult> GetPatients(
            [FromQuery] string? name = "",
            [FromQuery] List<Conclusion>? conclusions = null,
            [FromQuery] Sorting? sorting = null,
            [FromQuery] bool? scheduledVisits = false,
            [FromQuery] bool? onlyMine = false,
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

                var filteredPatients = _context.Patients
                            .Where(x => EF.Functions.ILike(x.Name, $"%{name}%"))
                            .ToList()
                            .OrderBy(x => x.Name);

                if (scheduledVisits == true)
                {
                    filteredPatients = filteredPatients
                        .Where(patient => _context.Inspections
                            .Any(i => i.PatientId == patient.Id && i.NextVisitDate != null && i.NextVisitDate > DateTime.UtcNow))
                        .OrderBy(patient => patient.Name);
                }
                if (onlyMine == true)
                {
                    filteredPatients = filteredPatients
                        .Where(patient => _context.Inspections
                            .Any(i => i.PatientId == patient.Id && i.Doctor.Id == doctorGuid))
                        .OrderBy(patient => patient.Name);
                }
                if ( conclusions != null && conclusions.Count() > 0)
                {
                filteredPatients = filteredPatients
                    .Where(patient => _context.Inspections
                        .Any(i => i.PatientId == patient.Id && conclusions.Contains(i.Conclusion)))
                    .OrderBy(patient => patient.Name);
                }
                if (sorting != null)
                {
                    switch (sorting)
                    {
                        case Sorting.NameAsc:
                            filteredPatients = filteredPatients.OrderBy(x => x.Name);
                            break;

                        case Sorting.NameDesc:
                            filteredPatients = filteredPatients.OrderByDescending(x => x.Name);
                            break;

                        case Sorting.InspectionDesc:
                            filteredPatients = filteredPatients
                                .OrderByDescending(patient => _context.Inspections
                                    .Where(i => i.PatientId == patient.Id)
                                    .Max(i => (DateTimeOffset?)i.CreateTime));
                            break;

                        case Sorting.InspectionAsc:
                            filteredPatients = filteredPatients
                                .OrderBy(patient => _context.Inspections
                                    .Where(i => i.PatientId == patient.Id)
                                    .Max(i => (DateTimeOffset?)i.CreateTime));
                            break;

                        case Sorting.CreateAsc:
                            filteredPatients = filteredPatients.OrderBy(x => x.CreateTime);
                            break;

                        case Sorting.CreateDesc:
                            filteredPatients = filteredPatients.OrderByDescending(x => x.CreateTime);
                            break;
                    }

                }


                PatientPagedListModel result = new PatientPagedListModel();

                int count = (int)Math.Ceiling((double)filteredPatients.Count() / size);

                       
                if (page > count)
                {
                    return BadRequest("Invalid page or size value.");
                }

                result.Pagination = new PageInfoModel(size, page, count);

                result.Patients = new List<PatientModel>();
                foreach (var patient in filteredPatients.Skip((page - 1) * size).Take(size))
                {
                    result.Patients.Add(new PatientModel(patient));
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseModel
                {
                    Status = "InternalServerError",
                    Message = $"{ex.Message}"
                });
            }
        }

        [HttpPost("{id}/inspections")]
        [ProducesResponseType(200, Type = typeof(Guid))]
        [ProducesResponseType(500, Type = typeof(ResponseModel))]
        public async Task<IActionResult> PostPatientsInspection(
            Guid id,
            [FromBody] InspectionCreateModel model
            )
        {
            try
            {
                var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                if (!User.Identity.IsAuthenticated || TokenBlackList.TokenBlacklisted(token))
                {
                    throw new UnauthorizedAccessException("User is not authorized");
                }

                var doctorIdParsed = Guid.TryParse(User.FindFirstValue(JwtRegisteredClaimNames.Jti), out var doctorGuid);
                if (!doctorIdParsed)
                {
                    throw new ArgumentException($"Could not parse DoctorId: {User.FindFirstValue(JwtRegisteredClaimNames.Jti)} into Guid");
                }

                if (_context.Inspections.Any(x => x.PatientId == id && x.Conclusion == Conclusion.Death))
                {
                    throw new ArgumentException($"No more inspections can be created for this patient after receiving conclusion: Death");
                }

                Inspection? parentInspection = _context.Inspections.Find(model.PreviousInspectionId);

                if (parentInspection != null)
                {
                    if (model.Date < parentInspection.Date || parentInspection.PatientId != id)
                    {
                        throw new ArgumentException("Invalid Date or PreviousInspectionId");
                    }
                    if (parentInspection.ChildInspection != null)
                    {
                        throw new ArgumentException($"Child inspection already exists for inspection with Id: {parentInspection.Id}");
                    }
                }
                else if (model.PreviousInspectionId != null)
                {
                    throw new ArgumentException("Invalid PreviousInspectionId");
                }

                if (model.Date > DateTime.UtcNow)
                {
                    throw new ArgumentException("Date can't be later than today");
                }

                if (model.Conclusion == Conclusion.Death)
                {
                    if (model.NextVisitDate != null || model.DeathDate == null)
                    {
                        throw new ArgumentException($"Inspections with conclusion: {model.Conclusion} must contain DeathDate and can't be scheduled after receiving conclusion");
                    }
                }
                else if (model.DeathDate != null)
                {
                    throw new ArgumentException($"Can not have DeathDate with conclusion: {model.Conclusion}");
                }

                if (model.Conclusion == Conclusion.Disease && model.NextVisitDate == null)
                {
                    throw new ArgumentException($"Must contain NextVisitDate after receiving conclusion: {model.Conclusion}");
                }

                int mainDiagnosesCount = model.diagnoses.Count(d => d.Type == DiagnosisType.Main);

                if (mainDiagnosesCount != 1)
                {
                    throw new ArgumentException("Exactly one diagnosis with type 'Main' is required.");
                }

                var diagnoses = model.diagnoses.Select(d => new Diagnosis
                {
                    Icd10RecordId = d.IcdDiagnosisId,
                    Description = d.Description,
                    Type = d.Type
                }).ToList();

                var uniqueSpecialityIds = new HashSet<Guid>();
                var consultations = model.consultations.Select(c =>
                {
                    if (!uniqueSpecialityIds.Add(c.SpecialityId))
                    {
                        throw new ArgumentException("Duplicate SpecialityId aren't allowed in consultations.");
                    }

                    var comment = new Comment
                    {
                        Content = c.Comment.Content,
                        AuthorId = doctorGuid
                    };

                    return new Consultation
                    {
                        SpecialityId = c.SpecialityId,
                        Comments = new List<Comment> { comment },
                    };
                }).ToList();

                var inspection = new Inspection
                {
                    Date = model.Date,
                    Anamnesis = model.Anamnesis,
                    Complaints = model.Complaints,
                    Treatment = model.Treatment,
                    Conclusion = model.Conclusion,
                    NextVisitDate = model.NextVisitDate,
                    DeathDate = model.DeathDate,
                    PreviousInspectionId = model.PreviousInspectionId,
                    Diagnoses = diagnoses,
                    Consultations = consultations,
                    DoctorId = doctorGuid,
                    PatientId = id
                };

                if (parentInspection != null)
                {
                    parentInspection.ChildInspection = inspection;
                }

                _context.Inspections.Add(inspection);
                _context.SaveChanges();

                return Ok(inspection.Id);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ResponseModel { Status = "BadRequest", Message = $"{ex.Message}" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new ResponseModel { Status = "Unauthorized", Message = $"{ex.Message}" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseModel { Status = "InternalServerError", Message = $"{ex.Message}" });
            }
        }


        [HttpGet("{id}/inspections")]
        [ProducesResponseType(200, Type = typeof(InspectionPagedListModel))]
        [ProducesResponseType(500, Type = typeof(ResponseModel))]
        public async Task<IActionResult> GetPatientsInspections(
            Guid id,
            [FromQuery] List<Guid>? icdRoots = null,
            [FromQuery] bool? grouped = false,
            [Range(1, Int32.MaxValue)] int page = 1,
            [Range(1, Int32.MaxValue)] int size = 5)
        {
            try
            {
                var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                if (!User.Identity.IsAuthenticated || TokenBlackList.TokenBlacklisted(token))
                {
                    throw new UnauthorizedAccessException("Unauthorized");
                }

                Patient? patient = await _context.Patients.FirstOrDefaultAsync(x => x.Id == id);

                if (patient == null)
                {
                    throw new KeyNotFoundException($"Patient with id:{id} was not found");
                }

                var doctorIdParsed = Guid.TryParse(User.FindFirstValue(JwtRegisteredClaimNames.Jti), out var doctorGuid);

                var filteredInspections = _context.Inspections
                    .Include(i => i.PreviousInspection) 
                    .Include(i => i.Diagnoses)
                        .ThenInclude(r => r.Icd10Record)
                    .Include(i => i.Consultations) 
                    .Include(i => i.Doctor) 
                    .Include(i => i.Patient) 
                    .Where(i => i.PatientId == id)
                    .ToList()
                    .OrderBy(i => i.CreateTime);

                if (grouped == true)
                {
                    filteredInspections = filteredInspections
                        .Where(x => x.PreviousInspectionId == null)
                        .OrderBy(x=> x.CreateTime);
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
                return BadRequest( new ResponseModel { Status = "Error", Message = $"{ex.Message}" });
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
        [ProducesResponseType(200, Type = typeof(PatientModel))]
        [ProducesResponseType(500, Type = typeof(ResponseModel))]
        public async Task<IActionResult> GetPatientsCard(
            Guid id)
        {
            try
            {
                var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                if (!User.Identity.IsAuthenticated || TokenBlackList.TokenBlacklisted(token))
                {
                    throw new UnauthorizedAccessException("Unauthorized");
                }

                var doctorIdParsed = Guid.TryParse(User.FindFirstValue(JwtRegisteredClaimNames.Jti), out var doctorGuid);

                Patient? patient = await _context.Patients.FirstOrDefaultAsync(x => x.Id == id);

                if (patient == null)
                {
                    throw new KeyNotFoundException($"Patient with id:{id} was not found");
                }

                var result = new PatientModel(patient);

                return Ok(result);
                        
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new ResponseModel
                {
                    Status = "Error",
                    Message = $"{ex.Message}"
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ResponseModel
                {
                    Status = "Error",
                    Message = $"Patient with id:{id} was not found"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseModel
                {
                    Status = "InternalServerError",
                    Message = $"{ex.Message}"
                });
            }
        }


        [HttpGet("{id}/inspections/search")]
        [ProducesResponseType(200, Type = typeof(List<InspectionShortModel>))]
        [ProducesResponseType(500, Type = typeof(ResponseModel))]
        public async Task<IActionResult> GetPatientsInspectionsSearch(
            Guid id,
            [FromQuery] string? request = ""
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
                if (!doctorIdParsed)
                {
                    throw new ArgumentException($"Could not parse DoctorId: {User.FindFirstValue(JwtRegisteredClaimNames.Jti)} into Guid");
                }

                var patientExists = await _context.Patients.AnyAsync(x => x.Id == id);
                if (!patientExists)
                {
                    throw new KeyNotFoundException($"Patient with id:{id} was not found");
                }

                var filteredInspections = await _context.Inspections
                    .Include(i => i.Diagnoses).ThenInclude(r => r.Icd10Record)
                    .Where(s =>
                        EF.Functions.ILike(s.Diagnoses.First(d => d.Type == DiagnosisType.Main).Icd10Record.Name, $"%{request}%") ||
                        (IcdService.IsICD10Code(request.ToUpper()) &&
                        EF.Functions.ILike(s.Diagnoses.First(d => d.Type == DiagnosisType.Main).Icd10Record.Code, $"{request}%")))
                    .Where(s => s.PreviousInspectionId == null && s.PatientId == id)
                    .OrderBy(i => i.CreateTime)
                    .ToListAsync();

                var result = filteredInspections.Select(inspection => new InspectionShortModel(inspection)).ToList();

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new ResponseModel
                {
                    Status = "Error",
                    Message = $"{ex.Message}"
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ResponseModel
                {
                    Status = "Error",
                    Message = $"Patient with id:{id} was not found"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseModel
                {
                    Status = "InternalServerError",
                    Message = $"{ex.Message}"
                });
            }
        }

    }

}
