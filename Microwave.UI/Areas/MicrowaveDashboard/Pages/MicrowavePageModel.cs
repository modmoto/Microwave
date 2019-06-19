using Microsoft.AspNetCore.Mvc.RazorPages;
using Microwave.Domain;

namespace Microwave.UI.Areas.MicrowaveDashboard.Pages
{
    public class MicrowavePageModel : PageModel
    {
        private readonly IMicrowaveConfiguration _configuration;

        public MicrowavePageModel(IMicrowaveConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string ServiceName => _configuration.ServiceName;
    }
}