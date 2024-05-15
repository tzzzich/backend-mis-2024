using Backend_aspnet_lab.dto;
using Backend_aspnet_lab.dto.Dictionary;
using Backend_aspnet_lab.Utils;
using Backend_aspnet_lab.Utils.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.Drawing.Printing;
using System.Linq;

namespace Backend_aspnet_lab.Controllers
{
    [Route("api/dictionary")]
    [Produces("application/json")]
    [ApiController]
    public class DictionaryController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        
        public DictionaryController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("speciality")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SpecialitiesPagedListModel))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResponseModel))]
        public async Task<ActionResult> Speciality(
            [FromQuery] string name = "",
            [Range(1, Int32.MaxValue)] int page = 1,
            [Range(1, Int32.MaxValue)] int size = 5)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var filteredSpecialities = _context.Specialities
                .Where(s => EF.Functions.ILike(s.Name, $"%{name}%"))
                .ToList()
                .OrderBy(s => s.Name);

                SpecialitiesPagedListModel result = new SpecialitiesPagedListModel();

                result.Pagination = Pagination.MakePagination(size, page, filteredSpecialities.Count());

                result.Specialties = new List<SpecialityModel>();
                foreach (var speciality in filteredSpecialities.Skip((page - 1) * size).Take(size))
                {
                    result.Specialties.Add(new SpecialityModel(speciality));
                }

                return Ok(result);

            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ResponseModel{Status = "Error", Message = $"{ex.Message}" });
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

        [HttpGet("icd10")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Icd10SearchModel))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResponseModel))]
        public async Task<ActionResult> Icd10(
            [FromQuery]string request = "", 
            [Range(1, Int32.MaxValue)]int page = 1,
            [Range(1, Int32.MaxValue)]int size = 5)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var filteredIcd = _context.Icd10Records
               .Where(s => EF.Functions.ILike(s.Name, $"%{request}%") 
               || IcdService.IsICD10Code(request.ToUpper()) && EF.Functions.ILike(s.Code, $"{request}%"))
               .ToList()
               .OrderBy(s => s.Code);

                Icd10SearchModel result = new Icd10SearchModel();

                result.Pagination = Pagination.MakePagination(size, page, filteredIcd.Count());

                foreach (var icd10 in filteredIcd.Skip((page - 1) * size).Take(size))
                {
                    result.Records.Add(new Icd10RecordModel(icd10));
                }

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ResponseModel { Status = "Error", Message = $"{ex.Message}" });
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

        [HttpGet("icd10/roots")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<Icd10RecordModel>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResponseModel))]
        public async Task<ActionResult> Icd10Roots()
        {
            try
            {
                List<Icd10RecordModel> result = new List<Icd10RecordModel>();

                var icdRoots = _context.Icd10Records
                   .Where(s => s.PreviousId == null)
                   .ToList()
                   .OrderBy(s => s.Code);

                foreach (var icd10 in icdRoots)
                {
                    result.Add(new Icd10RecordModel(icd10));
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

    }

}
