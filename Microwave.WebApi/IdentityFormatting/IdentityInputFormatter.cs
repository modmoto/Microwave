using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;

namespace Microwave.WebApi.IdentityFormatting
{
    public class IdentityInputFormatter : TextInputFormatter
    {
        public IdentityInputFormatter()
        {
            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("application/json"));

            SupportedEncodings.Add(Encoding.UTF8);
            SupportedEncodings.Add(Encoding.Unicode);
        }

        public override Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context, Encoding encoding)
        {
            using (var reader = new StreamReader(context.HttpContext.Request.Body, Encoding.UTF8))
            {
                string value = reader.ReadToEnd();
                var deserializeObject = JsonConvert.DeserializeObject(value, context.Metadata.ModelType, new IdentityConverter());
                return InputFormatterResult.SuccessAsync(deserializeObject);
            }
        }
    }
}