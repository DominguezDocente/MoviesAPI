using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MoviesAPI.ConfigurationOptions;
using MoviesAPI.Helpers;

namespace MoviesAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [DisableLimitRequests]
    public class ConfigurationsController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        //private readonly IOptions<PersonOptions> _personOptions;
        private readonly IOptionsSnapshot<PersonOptions> _personOptions;
        private readonly IOptionsMonitor<RatesOptions> _rateOptions;
        private readonly RatesPaymentProcess _ratesPaymentProcess;

        public ConfigurationsController(IConfiguration configuration, 
            IOptionsSnapshot<PersonOptions> personOptions 
            /*IOptions<PersonOptions> personOptions*/, 
            IOptionsMonitor<RatesOptions> rateOptions,
            RatesPaymentProcess ratesPaymentProcess)
        {
            _configuration = configuration;
            _personOptions = personOptions;
            _rateOptions = rateOptions;
            _ratesPaymentProcess = ratesPaymentProcess;
        }

        public ActionResult Get()
        {
            string option1 = _configuration["AppName"];
            string option2 = _configuration.GetValue<string>("AppName");

            string sectionOp1 = _configuration["ConnectionStrings:MyConnection"];
            string sectionOp2 = _configuration.GetValue<string>("ConnectionStrings:MyConnection");
            IConfigurationSection section = _configuration.GetSection("ConnectionStrings");
            string sectionOp3 = section.GetValue<string>("MyConnection");

            IConfigurationSection jwt = _configuration.GetSection("jwt");
            var jwtSection = new
            {
                key = jwt.GetValue<string>("key"),
                TTL = jwt.GetValue<int>("TTL"),
            };

            return Ok(new
            {
                first = new 
                {
                    op1 = option1,
                    op2 = option2 
                },

                sections = new
                {
                    op1 = sectionOp1,
                    op2 = sectionOp2,
                    op3 = sectionOp3,
                },

                jwtSection
            });
        }

        [HttpGet("get-all")]
        public ActionResult GetAll()
        {
            var children = _configuration.GetChildren().Select(x => $"{x.Key}: {x.Value}");
            var logginChildren = _configuration.GetSection("Logging").GetChildren().Select(x => $"{x.Key}: {x.Value}");
            var authorChildren = _configuration.GetSection("AuthorSection").GetChildren().Select(x => $"{x.Key}: {x.Value}");

            return Ok(new
            {
                children,
                logginChildren,
                authorChildren
            });
        }

        [HttpGet("providers")]
        public ActionResult GetProviders()
        {
            string value = _configuration.GetValue<string>("WhoAmI");
            return Ok(value);
        }

        [HttpGet("author-section")]
        public ActionResult GetAuthorSection()
        {
            return Ok(_personOptions.Value);
        }

        [HttpGet("rates-section")]
        public ActionResult GetRatesSection()
        {
            return Ok(_rateOptions);
        }

        [HttpGet("rates-section-monitor")]
        public ActionResult GetRatesSection2()
        {
            return Ok(_ratesPaymentProcess.GetRates());
        }
    }
}
