using Microsoft.AspNetCore.Mvc.RazorPages;
using Microwave.WebApi;

namespace Microwave.UI.Areas.MicrowaveDashboard.Pages
{
    public class MicrowavePageModel : PageModel
    {
        private readonly MicrowaveWebApiConfiguration _configuration;

        public MicrowavePageModel(MicrowaveWebApiConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string ServiceName => _configuration.ServiceName;
    }
}