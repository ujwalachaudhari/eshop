using CatalogAPI.Models;
using Microsoft.AspNetCore.Mvc.Formatters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace CatalogAPI.CustomFormatters
{
    public class CsvOutputFormatter : TextOutputFormatter
    {
        public CsvOutputFormatter()
        {
            this.SupportedEncodings.Add(Encoding.UTF8);
            this.SupportedEncodings.Add(Encoding.Unicode);
            this.SupportedMediaTypes.Add("text/csv");
            this.SupportedMediaTypes.Add("application/csv");
        }

        protected override bool CanWriteType(Type type)
        {
            if(typeof(CatalogItem).IsAssignableFrom(type) || typeof(IEnumerable<CatalogItem>).IsAssignableFrom(type))
            {
                return true;
            }
            return false;
        }

        public override async Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
        {
            //Write code to convert Catalogitem type to CSV
            var buffer = new StringBuilder();
            var response = context.HttpContext.Response;
            if(context.Object is CatalogItem)
            {
                var item = context.Object as CatalogItem;
                buffer.Append("Id,Name,Price,Quantity,ReorderLevel,ManufacturingDate,ImageUrl" + Environment.NewLine);
                buffer.Append($"{item.Id},{item.Name},{item.Price},{item.Quantity},{item.ReorderLevel},{item.ManufacturingDate},{item.ImageUrl}");
            }
            else if(context.Object is IEnumerable<CatalogItem>)
            {
                var items = context.Object as IEnumerable<CatalogItem>;
                buffer.Append("Id,Name,Price,Quantity,ReorderLevel,ManufacturingDate,ImageUrl" + Environment.NewLine);
                foreach(var item in items)
                {
                    buffer.Append($"{item.Id},{item.Name},{item.Price},{item.Quantity},{item.ReorderLevel},{item.ManufacturingDate},{item.ImageUrl}{Environment.NewLine}");
                }
            }
            await response.WriteAsync(buffer.ToString(),selectedEncoding); //using Microsoft.AspNetCore.Http
        }
    }
}
