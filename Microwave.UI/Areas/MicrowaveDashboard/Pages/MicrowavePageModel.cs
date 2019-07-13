using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Microwave.UI.Areas.MicrowaveDashboard.Pages
{
    public class MicrowavePageModel : PageModel
    {
        private readonly MicrowaveUiConfiguration _configuration;

        public MicrowavePageModel(MicrowaveUiConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string ServiceName => _configuration.ServiceName;
    }
}