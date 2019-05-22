using Microsoft.AspNetCore.Mvc.RazorPages;
using Microwave.Domain;

namespace Microwave.Pages
{
    public class MicrowavePageModel : PageModel
    {
        private readonly MicrowaveConfiguration _configuration;

        public MicrowavePageModel(MicrowaveConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string ServiceName => _configuration.ServiceName;
    }
}