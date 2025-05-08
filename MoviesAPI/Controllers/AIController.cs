using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MoviesAPI.Helpers;
using MoviesAPI.Services.Keyed;

namespace MoviesAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [DisableLimitRequests]
    public class AIController : ControllerBase
    {
        private readonly IAIService _openAiService;
        private readonly IAIService _geminiService;

        public AIController([FromKeyedServices("openai")] IAIService openAiService, [FromKeyedServices("gemini")] IAIService geminiService)
        {
            _openAiService = openAiService;
            _geminiService = geminiService;
        }

        [HttpGet]
        public List<string> Get()
        {
            return new List<string>
            {
                _openAiService.GenerateArticle(), 
                _geminiService.GenerateArticle()
            };
        }
    }
}
