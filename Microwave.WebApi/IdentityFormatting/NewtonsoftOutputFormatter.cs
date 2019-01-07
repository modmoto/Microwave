using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;

namespace Microwave.WebApi.IdentityFormatting
{
    public class NewtonsoftOutputFormatter : TextOutputFormatter
    {
        public NewtonsoftOutputFormatter()
        {
            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("application/json"));

            SupportedEncodings.Add(Encoding.UTF8);
            SupportedEncodings.Add(Encoding.Unicode);
        }

        public override async Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
        {
            var ser = JsonConvert.SerializeObject(context.Object,
                new IdentityConverter(),
                new DateTimeOffsetConverter());
            await context.HttpContext.Response.WriteAsync(ser);
        }
    }
}