using Microsoft.Extensions.Options;

namespace MoviesAPI.ConfigurationOptions
{
    public class RatesPaymentProcess
    {
        private RatesOptions _ratesOptions;

        public RatesPaymentProcess(IOptionsMonitor<RatesOptions> ratesMonitor)
        {
            _ratesOptions = ratesMonitor.CurrentValue;

            ratesMonitor.OnChange(newFee =>
            {
                _ratesOptions = newFee;
            });
        }        

        public void ProcessPayment()
        {

        }

        public RatesOptions GetRates()
        {
            return _ratesOptions;
        }
    }
}
