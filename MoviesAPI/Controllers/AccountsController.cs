using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MoviesAPI.DTOs;
using MoviesAPI.Entities;
using MoviesAPI.Helpers;
using MoviesAPI.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MoviesAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [DisableLimitRequests]
    public class AccountsController : CustomBaseController
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IKeysService _keysService;

        public AccountsController(UserManager<User> userManager,
                                  SignInManager<User> signInManager,
                                  IConfiguration configuration,
                                  ApplicationDbContext context,
                                  IMapper mapper,
                                  IKeysService keysService) : base(context, mapper)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _context = context;
            _mapper = mapper;
            _keysService = keysService;
        }

        [HttpPost("CreateUser")]
        public async Task<ActionResult<UserToken>> CreateUser([FromBody] UserInfo model)
        {
            User user = new User { UserName = model.Email, Email = model.Email };
            IdentityResult result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                return await BuildToken(model, user.Id);
            }
            else
            {
                return BadRequest(result.Errors);
            }
        }

        [HttpPost("Register")]
        public async Task<ActionResult<UserToken>> Registrar(UserInfo userInfo)
        {
            User user = new User
            {
                UserName = userInfo.Email,
                Email = userInfo.Email
            };

            IdentityResult resultado = await _userManager.CreateAsync(user, userInfo.Password!);

            if (resultado.Succeeded)
            {
                UserToken authenticationResponse = await BuildToken(userInfo, user.Id);
                await _keysService.CreateKey(user.Id, KeyType.Free);
                return authenticationResponse;
            }
            else
            {
                foreach (var error in resultado.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return ValidationProblem();
            }
        }

        [HttpPost("Login")]
        public async Task<ActionResult<UserToken>> Login([FromBody] UserInfo model)
        {
            Microsoft.AspNetCore.Identity.SignInResult result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, isPersistent: false, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                User user = await _userManager.FindByEmailAsync(model.Email);
                return await BuildToken(model, user.Id);
            }
            else
            {
                return BadRequest("Credenciales inválidas.");
            }
        }

        [HttpPost("RenewToken")]
        public async Task<ActionResult<UserToken>> RenewToken()
        {
            UserInfo userInfo = new UserInfo
            {
                Email = HttpContext.User.Identity.Name
            };

            //IdentityUser user = await _userManager.GetUserAsync(HttpContext.User);
            User user = await _userManager.FindByEmailAsync(userInfo.Email);

            return await BuildToken(userInfo, user.Id);
        }

        [HttpGet("Users")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public async Task<ActionResult<List<UserDTO>>> GetUsers([FromQuery] PaginationDTO dto)
        {
            IQueryable<User> queryable = _context.Users.AsQueryable();
            queryable = queryable.OrderBy(x => x.Email);
            return await GetPagination<User, UserDTO>(dto);
        }

        [HttpGet("Roles")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public async Task<ActionResult<List<string>>> GetRoles()
        {
            return await _context.Roles.Select(r => r.Name).ToListAsync();
        }

        [HttpGet("AssingRole")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public async Task<ActionResult> AssingRole(EditRoleDTO dto)
        {
            User user = await _userManager.FindByIdAsync(dto.UserId);

            if (user is null)
            {
                return NotFound();
            }

            //await _userManager.AddToRoleAsync(dto.RoleName);
            await _userManager.AddClaimAsync(user, new Claim(ClaimTypes.Role, dto.RoleName));
            return NoContent();
        }

        [HttpGet("RemoveRole")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public async Task<ActionResult> RemoveRole(EditRoleDTO dto)
        {
            User user = await _userManager.FindByIdAsync(dto.UserId);

            if (user is null)
            {
                return NotFound();
            }

            await _userManager.RemoveClaimAsync(user, new Claim(ClaimTypes.Role, dto.RoleName));
            return NoContent();
        }

        private async Task<UserToken> BuildToken(UserInfo model, string userId)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, model.Email),
                new Claim(ClaimTypes.Email, model.Email),
                new Claim("userId", userId)
            };

            User identityUser = await _userManager.FindByEmailAsync(model.Email);

            claims.Add(new Claim(ClaimTypes.NameIdentifier, identityUser.Id));

            IList<Claim> claimsDB = await _userManager.GetClaimsAsync(identityUser);

            claims.AddRange(claimsDB);

            SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["jwt:key"]));
            SigningCredentials creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            DateTime expiration = DateTime.UtcNow.AddYears(1);

            JwtSecurityToken token = new JwtSecurityToken(
                issuer: null,
                audience: null,
                claims: claims,
                expires: expiration,
                signingCredentials: creds
            );

            return new UserToken()
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Expiration = expiration
            };
        }
    }
}
