using Backend_aspnet_lab.dto;
using Backend_aspnet_lab.dto.DoctorController;
using Backend_aspnet_lab.Models;
using Backend_aspnet_lab.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NuGet.Common;
using Swashbuckle.AspNetCore.Annotations;
using System.Diagnostics.Metrics;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;

namespace Backend_aspnet_lab.Controllers
{
    [Route("api/doctor")]
    [Produces("application/json")]
    [ApiController]
    public class DoctorController : ControllerBase
    {

        private readonly UserManager<Doctor> _userManager;

        private readonly SignInManager<Doctor> _signInManager;

        private readonly ILogger<DoctorController> _logger;

        private readonly IConfiguration _configuration;

        public DoctorController(UserManager<Doctor> userManager, SignInManager<Doctor> signInManager, 
            IConfiguration configuration, ILogger<DoctorController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TokenResponseModel))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResponseModel))]
        public async Task<IActionResult> Login(
            [FromBody] LoginCredentialsModel model
            )
        {
            try
            {
                if (!ModelState.IsValid) { return BadRequest(ModelState); }

                var user = await _userManager.FindByNameAsync(model.Email);

                if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
                {

                    var token = GenerateToken(user);

                    return Ok(new TokenResponseModel(token));
                }

                throw new ArgumentException("Incorrect Login Credentials");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ResponseModel
                {
                    Status = "InternalServerError",
                    Message = $"{ex.Message}"
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

        [HttpPost("register")]
        [ProducesResponseType(200, Type = typeof(TokenResponseModel))]
        [ProducesResponseType(500, Type = typeof(ResponseModel))]
        public async Task<IActionResult> Register(
            [FromBody] DoctorRegisterModel model
            )
        {
            try
            {
                if (!ModelState.IsValid) { return BadRequest(ModelState); }

                var user = new Doctor
                {
                    Name = model.Name,
                    Email = model.Email,
                    UserName = model.Email,
                    Birthday = model.Birthday,
                    Gender = model.Gender,
                    PhoneNumber = model.Phone,
                    SpecialityId = model.Speciality
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    var token = GenerateToken(user);

                    return Ok(new TokenResponseModel(token));
                }
                else
                {
                    return StatusCode(StatusCodes.Status400BadRequest, IdentityResult.Failed(result.Errors.ToArray()));

                }
            }
            catch
            {
                return StatusCode(500, new ResponseModel
                {
                    Status = "InternalServerError",
                    Message = "Speciality with the stated Id doesn't exist."
                });
            }
        }

        [Authorize]
        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResponseModel))]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                if (User.Identity.IsAuthenticated && !TokenBlackList.TokenBlacklisted(token))
                {
                    await _signInManager.SignOutAsync();
                    TokenBlackList.AddToBlacklist(token);
                    return Ok(new ResponseModel
                    {
                        Message = "Logged out."
                    });
                }
                else
                {
                    throw new UnauthorizedAccessException("User is not authorized");
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized (new ResponseModel
                {
                    Status = "InternalServerError",
                    Message = $"{ex.Message}"
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

        [Authorize]
        [HttpGet("profile")]
        
        public async Task<ActionResult<DoctorModel>> Profile()
        {
            try
            {
                var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                if (User.Identity.IsAuthenticated && !TokenBlackList.TokenBlacklisted(token))
                {
                    var userId = User.FindFirstValue(JwtRegisteredClaimNames.Jti);

                    if (Guid.TryParse(userId, out var userGuid))
                    {
                        var user = await _userManager.FindByIdAsync(userId);

                        if (user != null)
                        {
                            try
                            {
                                var result = new DoctorModel(user);
                                return Ok(result);
                            }
                            catch (Exception ex)
                            {
                                throw new ArgumentException(ex.Message);
                            }


                        }
                        else
                        {
                            throw new KeyNotFoundException($"Doctor with Id {userId} not found");
                        }
                    }
                    else
                    {
                        throw new ArgumentException($"Could not parse DoctorId: {userId} into Guid");
                    }
                }
                else
                {
                    throw new UnauthorizedAccessException("User is not authorized");
                }
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ResponseModel
                {
                    Status = "Error",
                    Message = $"{ex.Message}"
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest( new ResponseModel
                {
                    Status = "Error",
                    Message = $"{ex.Message}"
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new ResponseModel
                {
                    Status = "Error",
                    Message = $"{ex.Message}"
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

        [Authorize]
        [HttpPut("profile")]
        public async Task<ActionResult<DoctorModel>> Profile(
            [FromBody] DoctorEditModel model
            )
        {
            try
            {
                var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                if (User.Identity.IsAuthenticated && !TokenBlackList.TokenBlacklisted(token))
                {
                    var userId = User.FindFirstValue(JwtRegisteredClaimNames.Jti);

                    if (Guid.TryParse(userId, out var userGuid))
                    {
                        var user = await _userManager.FindByIdAsync(userId);

                        if (user != null)
                        {

                            if (!ModelState.IsValid) { return BadRequest(ModelState); }

                            try
                            {
                                user.Name = model.Name;
                                user.Email = model.Email;
                                user.UserName = model.Email;
                                user.Birthday = model.Birthday;
                                user.Gender = model.Gender;
                                user.PhoneNumber = model.Phone;

                                await _userManager.UpdateAsync(user);

                                var result = new DoctorModel(user);

                                return Ok(result);
                            }
                            catch (Exception ex)
                            {
                                throw new ArgumentException(ex.Message);
                            }
                        }
                        else
                        {
                            throw new KeyNotFoundException($"Doctor with Id {userId} not found");
                        }
                    }
                    else
                    {
                        throw new ArgumentException($"Could not parse DoctorId: {userId} into Guid");
                    }
                }
                else
                {
                    throw new UnauthorizedAccessException("User is not authorized");
                }
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ResponseModel
                {
                    Status = "Error",
                    Message = $"{ex.Message}"
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ResponseModel
                {
                    Status = "Error",
                    Message = $"{ex.Message}"
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new ResponseModel
                {
                    Status = "Error",
                    Message = $"{ex.Message}"
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

        private string GenerateToken(Doctor user)
        {
            return new JwtSecurityTokenHandler().WriteToken(CreateJwtToken(_configuration, user));
        }

        private JwtSecurityToken? CreateJwtToken(IConfiguration _configuration, Doctor user)
        {
            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_configuration["JwtSettings:SecretKey"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Name),
                new Claim(JwtRegisteredClaimNames.Jti, user.Id.ToString())
            };

            var token = new JwtSecurityToken(
                _configuration["JwtSettings:Issuer"],
                _configuration["JwtSettings:Audience"],
                claims,
                expires: DateTime.Now.AddMinutes(Convert.ToDouble(_configuration["JwtSettings:ExpirationInMinutes"])),
                signingCredentials: credentials
            );

            return token;
        }

    }
}
