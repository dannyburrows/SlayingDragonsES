using System.Web.Http;
using WebActivatorEx;
using Elasticsearch;
using Swashbuckle.Application;
using System;
using System.Linq;

[assembly: PreApplicationStartMethod(typeof(SwaggerConfig), "Register")]

namespace Elasticsearch
{
    public class SwaggerConfig
    {
        public static void Register()
        {
            var thisAssembly = typeof(SwaggerConfig).Assembly;

            GlobalConfiguration.Configuration 
                .EnableSwagger(c =>
                    {
                        c.SingleApiVersion("v1", "Elasticsearch");
                        c.DescribeAllEnumsAsStrings(true);
                        c.GroupActionsBy(apiDesc => apiDesc.ActionDescriptor.ControllerDescriptor.GetCustomAttributes<ControllerNameAttribute>().Select(n => n.Name).FirstOrDefault());
                        c.IncludeXmlComments(GetXmlCommentsPath());
                        c.UseFullTypeNameInSchemaIds( );
                    })
                .EnableSwaggerUi(c =>
                    {
                        // Use the "InjectStylesheet" option to enrich the UI with one or more additional CSS stylesheets.
                        // The file must be included in your project as an "Embedded Resource", and then the resource's
                        // "Logical Name" is passed to the method as shown below.
                        //
                        //c.InjectStylesheet(containingAssembly, "Swashbuckle.Dummy.SwaggerExtensions.testStyles1.css");
                    });
        }

        private static string GetXmlCommentsPath()
        {
            var path = String.Format(@"{0}bin\Elasticsearch.XML", AppDomain.CurrentDomain.BaseDirectory);
            return path;
        }
    }
}
