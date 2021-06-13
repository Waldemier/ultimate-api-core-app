using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Entities.DataTransferObjects;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;

namespace UltimateWebApi.Csv
{
    public class CsvOutputFormatter: TextOutputFormatter
    {
        /// <summary>
        ///     In the constructor, we define which media type this formatter should parse as well as encodings.
        /// </summary>
        public CsvOutputFormatter()
        {
            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("text/csv"));
            SupportedEncodings.Add(Encoding.UTF8);
            SupportedEncodings.Add(Encoding.Unicode);
        }

        /// <summary>
        ///     The CanWriteType method is overridden, and it indicates whether or not the CompanyDto type can be written by this serializer.
        /// </summary>
        protected override bool CanWriteType(Type type)
        {
            if(typeof(CompanyDto).IsAssignableFrom(type) || typeof(IEnumerable<CompanyDto>).IsAssignableFrom(type))
            {
                return base.CanWriteType(type);
            }
            return false;
        }

        /// <summary>
        ///     The WriteResponseBodyAsync method constructs the response.
        /// </summary>
        public override async Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
        {
            var buffer = new StringBuilder();
            if(context.Object is IEnumerable<CompanyDto>)
            {
                foreach (var company in (IEnumerable<CompanyDto>)context.Object)
                {
                    FormatCsv(buffer, company);
                }
            }
            else
            {
                FormatCsv(buffer, (CompanyDto)context.Object);
            }
            await context.HttpContext.Response.WriteAsync(buffer.ToString());
        }

        /// <summary>
        ///     Formats a response the way we want it.
        /// </summary>
        private static void FormatCsv(StringBuilder buffer, CompanyDto company)
        {
            buffer.AppendLine($"{company.Id},\"{company.Name},\"{company.FullAddress}\"");
        }
    }
}
