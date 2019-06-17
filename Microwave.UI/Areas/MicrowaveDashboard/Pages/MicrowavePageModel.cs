using Microsoft.AspNetCore.Mvc.RazorPages;
using Microwave.Domain;

namespace Microwave.UI.Areas.MicrowaveDashboard.Pages
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