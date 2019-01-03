using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;

namespace Microwave.WebApi
{
    public class IdentityFormatter : TextOutputFormatter
    {
        public IdentityFormatter()
        {
            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("application/json"));

            SupportedEncodings.Add(Encoding.UTF8);
            SupportedEncodings.Add(Encoding.Unicode);
        }

        public override async Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
        {
            var ser = JsonConvert.SerializeObject(context.Object, new IdentityConverter());
            await context.HttpContext.Response.WriteAsync(ser);
        }
    }

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