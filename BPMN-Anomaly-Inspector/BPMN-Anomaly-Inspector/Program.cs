
using AnomalyDection.Core.Benchmark;
using BenchmarkDotNet.Running;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;

namespace BPMN_Anomaly_Inspector
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "ISE @ NYCU :Detect Anomaly API", Version = "v1" });
                c.OperationFilter<FileUploadOperationFilter>(); // Add this line
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthorization();


            app.MapControllers();
           
            app.Run();
        }
    }


    public class FileUploadOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var fileUploadParams = context.MethodInfo.GetParameters()
                .Where(p => p.ParameterType == typeof(IFormFile))
                .Select(p => p.Name)
                .ToList();

            if (fileUploadParams.Any())
            {
                operation.RequestBody = new OpenApiRequestBody
                {
                    Content = {
                    ["multipart/form-data"] = new OpenApiMediaType {
                        Schema = new OpenApiSchema {
                            Type = "object",
                            Properties = fileUploadParams.ToDictionary(
                                name => name,
                                name => new OpenApiSchema { Type = "string", Format = "binary" })
                        }
                    }
                }
                };
            }
        }
    }

}
