using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;
using MyCNBlog.Core;

namespace MyCNBlog.Api
{
    /// <summary>
    /// 支持客户端传纯文本 text/plain
    /// 居然没有内置的支持...
    /// </summary>
    public class StringInputTextFormatter : TextInputFormatter
    {
        public readonly static string ContentType = ContentTypes.Text;

        public StringInputTextFormatter()
        {
            SupportedMediaTypes.Add(new MediaTypeHeaderValue(ContentType));
            SupportedEncodings.Add(Encoding.UTF8);
            SupportedEncodings.Add(Encoding.Unicode);
            SupportedEncodings.Add(Encoding.BigEndianUnicode);
        }

        public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context, Encoding encoding)
        {
            using(var reader = new StreamReader(context.HttpContext.Request.Body))
            {
                return await InputFormatterResult.SuccessAsync(await reader.ReadToEndAsync());
            }
        }

        public override bool CanRead(InputFormatterContext context)
        {
            string contentType = context.HttpContext.Request.ContentType;
            return contentType.StartsWith(ContentType);
        }
    }
}
