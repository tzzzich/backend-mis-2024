using Backend_aspnet_lab.dto.PatientController;
using Backend_aspnet_lab.dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Backend_aspnet_lab.Models;
using Backend_aspnet_lab.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Drawing;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using Backend_aspnet_lab.dto.Dictionary;
using NuGet.Packaging.Signing;
using Backend_aspnet_lab.Utils.Service;

namespace Backend_aspnet_lab.Controllers
{

    [Route("api/report")]
    [Produces("application/json")]
    [ApiController]
    [Authorize]
    public class ReportController : ControllerBase
    {

        private readonly ApplicationDbContext _context;
        private readonly UserManager<Doctor> _userManager;

        public ReportController(UserManager<Doctor> userManager, ApplicationDbContext context)
        {
            _context = context;
            _userManager = userManager;
            //_service = new InspectionService();
        }

        [HttpGet("icdrootsreport")]
        [ProducesResponseType(200, Type = typeof(IcdRootsReportModel))]
        [ProducesResponseType(500, Type = typeof(ResponseModel))]
        public async Task<IActionResult> GetReports(
            [FromQuery] DateTimeOffset start,
            [FromQuery] DateTimeOffset end,
            [FromQuery] List<Guid>? icdRoots = null
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


                var icdRootsDb = icdRoots != null && icdRoots.Count > 0
                    ? _context.Icd10Records.Where(s => s.PreviousId == null && icdRoots.Contains(s.Id)).ToList().OrderBy(s => s.Code)
                    : _context.Icd10Records.Where(s => s.PreviousId == null).ToList().OrderBy(s => s.Code);

                if (icdRoots != null && icdRoots.Count > 0)
                {
                    foreach (var root in icdRoots)
                    {
                        if (icdRootsDb.FirstOrDefault(x => x.Id == root) == null)
                        {
                            throw new ArgumentException($"{root} is not a root Icd10");
                        }
                    }
                }
                else
                {
                    icdRoots.AddRange(icdRootsDb.Select(ir => ir.Id));
                }

                var filteredInspections = _context.Inspections
                    .Include(i => i.PreviousInspection)
                    .Include(i => i.Diagnoses)
                        .ThenInclude(r => r.Icd10Record)
                    .Include(i => i.Consultations)
                    .Include(i => i.Patient)
                    .Where(i => i.CreateTime >= start && i.CreateTime <= end).ToList();

                filteredInspections = filteredInspections
                    .Where(i => icdRoots
                        .Contains(IcdService.FindIcdRoot(i.Diagnoses
                            .Where(d => d.Type == DiagnosisType.Main)
                            .Select(diag => diag.Icd10Record).FirstOrDefault()))).ToList();

                var result = new IcdRootsReportModel();

                result.Filters = new IcdRootsReportFiltersModel { End = end, Start = start, IcdRoots = icdRootsDb.Where(ir => icdRoots.Contains(ir.Id)).Select(rec => rec.Code).ToList() };
                result.SummaryByRoot = new Dictionary<string, int>();
                foreach (var root in icdRootsDb)
                {
                    result.SummaryByRoot.Add(root.Code, 0);
                }

                result.Records = new List<IcdRootsReportRecordModel>();

                foreach (var patient in filteredInspections.Select(i => i.Patient).ToHashSet())
                {
                    var item = new IcdRootsReportRecordModel();
                    item.VisitsByRoot = new Dictionary<string, int>();
                    var inspectionsByPatient = filteredInspections.Where(ins => ins.PatientId == patient.Id).ToList();
                    foreach (var root in icdRootsDb) 
                    {
                        int countByRoot = inspectionsByPatient.Where(insp => root.Id == IcdService.FindIcdRoot(
                                                                insp.Diagnoses.Where(diag => diag.Type == DiagnosisType.Main)
                                                                .FirstOrDefault().Icd10Record))
                                                              .Count();
                        if (countByRoot > 0)
                        {
                            item.VisitsByRoot.Add(root.Code, countByRoot);
                            result.SummaryByRoot[root.Code] += countByRoot;
                        }
                    }
                    item.PatientBirthdate = patient.Birthday;
                    item.PatientName = patient.Name;
                    item.Gender = patient.Gender;
                    result.Records.Add(item);
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
                return NotFound(new ResponseModel { Status = "Error", Message = $"{ex.Message}" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseModel { Status = "InternalServerError", Message = $"{ex.Message}" });
            }
        }
    }
}

