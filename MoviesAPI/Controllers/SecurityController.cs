using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MoviesAPI.DTOs;
using MoviesAPI.Helpers;
using MoviesAPI.Services;
using System.Security.Cryptography;

namespace MoviesAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [DisableLimitRequests]
    public class SecurityController : ControllerBase
    {
        private IDataProtector _dataProtector;
        private ITimeLimitedDataProtector _limitedDataProtector;
        private readonly IHashService _hashService;

        public SecurityController(IDataProtectionProvider protectionProvider, IHashService hashService)
        {
            _dataProtector = protectionProvider.CreateProtector("SecurityController");
            _limitedDataProtector = _dataProtector.ToTimeLimitedDataProtector();
            _hashService = hashService;
        }

        [HttpGet("encrypt")]
        public ActionResult Encrypt(string plainText)
        {
            string encryptedText = _dataProtector.Protect(plainText);
            return Ok(new { encryptedText });
        }

        [HttpGet("decrypt")]
        public ActionResult Decrypt(string encryptedText)
        {
            string plainText = _dataProtector.Unprotect(encryptedText);
            return Ok(new { plainText });
        }

        [HttpGet("encrypt-by-time")]
        public ActionResult EncryptByTime(string plainText)
        {
            string encryptedText = _limitedDataProtector.Protect(plainText, lifetime: TimeSpan.FromSeconds(30));
            return Ok(new { encryptedText });
        }

        [HttpGet("decrypt-by-time")]
        public ActionResult DecryptByTime(string encryptedText)
        {
            try
            {
                string plainText = _limitedDataProtector.Unprotect(encryptedText);
                return Ok(new { plainText });
            }
            catch(CryptographicException ex)
            {
                return BadRequest("Ha expirado el tiempo para desencriptar");
            }

        }

        [HttpGet("list/expiring-token")]
        public ActionResult GetExpiringTokenForList()
        {
            string plainText = Guid.NewGuid().ToString();
            string token = _limitedDataProtector.Protect(plainText, lifetime: TimeSpan.FromSeconds(30));

            string scheme = Request?.Scheme ?? "https";
            string url = Url.RouteUrl("GetListUsingToken", new { token }, scheme);

            return Ok(new { url});
        }

        [HttpGet("list/{token}", Name = "GetListUsingToken")]
        [AllowAnonymous] // cuando esté en controlador con protección
        public ActionResult GetListUsingToken(string token)
        {
            {
                try
                {
                    _limitedDataProtector.Unprotect(token);
                }
                catch (Exception)
                {
                    ModelState.AddModelError(nameof(token), "El token ha expirado.");
                    return ValidationProblem();
                }

                List<Dictionary<string, string>> list = new List<Dictionary<string, string>>
                    {
                        new Dictionary<string, string>
                        {
                            {"Id", "1" },
                            { "Name", "Sun" },
                        },
                        new Dictionary<string, string>
                        {
                            {"Id", "2" },
                            { "Name", "Moon" },
                        },
                        new Dictionary<string, string>
                        {
                            {"Id", "3" },
                            { "Name", "Mercury" },
                        },
                        new Dictionary<string, string>
                        {
                            {"Id", "4" },
                            { "Name", "Venus" },
                        },
                        new Dictionary<string, string>
                        {
                            {"Id", "5" },
                            { "Name", "Earth" },
                        },
                        new Dictionary<string, string>
                        {
                            {"Id", "6" },
                            { "Name", "Mars" },
                        },
                        new Dictionary<string, string>
                        {
                            {"Id", "7" },
                            { "Name", "Jupiter" },
                        },
                    };

                return Ok(new { list });
            }
        }

        [HttpGet("hash")]
        public ActionResult Hash(string plainText)
        {
            HashResultDTO hash1 = _hashService.Hash(plainText);
            HashResultDTO hash2 = _hashService.Hash(plainText);
            HashResultDTO hash3 = _hashService.Hash(plainText, hash2.Salt);

            return Ok(new
            {
                result = new { hash1, hash2, hash3 }
            });
        }
    }
}
