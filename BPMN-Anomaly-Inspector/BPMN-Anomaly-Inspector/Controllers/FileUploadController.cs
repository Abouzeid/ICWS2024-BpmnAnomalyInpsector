using AnomalyDection.Core.Benchmark;
using AnomalyDection.Core.ConcurrentAnomaly;
using AnomalyDection.Core.SP_Tree;
using BenchmarkDotNet.Running;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net;
using System.Text.Json;
using System.Xml;

namespace BPMN_Anomaly_Inspector.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FileUploadController : ControllerBase
    {

        private readonly ILogger<FileUploadController> _logger;

        public FileUploadController(ILogger<FileUploadController> logger)
        {
            _logger = logger;
        }

        [HttpPost("UploadBPMN")]
        public async Task<IActionResult> UploadBPMN([FromForm] IFormFile file, bool isPrevApproach = false)
        {
            // Check if the file is present
            if (file == null || file.Length == 0)
                return BadRequest("Please upload a file.");

            // Check for the correct file extension (.bpmn)
            if (!Path.GetExtension(file.FileName).Equals(".bpmn", StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { message = "Only bpmn is allowed!" });

            XmlDocument bpmnDocument = new XmlDocument();
            try
            {
                SpTree_Node sptree = null;
                // Load the uploaded file into XmlDocument
                using (var stream = file.OpenReadStream())
                {
                    sptree = ConvertBpmnIntoSpTree.Parse(stream);
                    //bpmnDocument.Load(stream);
                }

                string result = string.Empty;
                if (isPrevApproach)
                {
                    Prev_SP_Tree_Approach prevApproach = new Prev_SP_Tree_Approach();

                    prevApproach.Original_sptree_CAD(sptree);
                    var output = prevApproach.GetAnomalies();
                    result = JsonConvert.SerializeObject(output);
                }
                else
                {
                    OurApproachToConcurrent ourApproach = new OurApproachToConcurrent();
                    ourApproach.Traverse(sptree);
                    result = JsonConvert.SerializeObject(ourApproach.GetAnomalyResult());
                }
                if (!string.IsNullOrEmpty(result))
                    return new ContentResult()
                    {
                        Content = result,
                        ContentType = "application/json",
                        StatusCode = (int)HttpStatusCode.BadRequest
                    };
                else
                {
                    return Ok("Success and free from artifact anomaly!");
                }
            }
            catch (XmlException ex)
            {
                // Handle XML parsing errors (e.g., file is not a valid XML)
                return BadRequest($"Error loading BPMN file: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Handle other potential errors
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        [HttpGet("benchmark")]
        public IActionResult GenerateBenchmark()
        {
            Task.Run(() =>
            {
               BenchmarkRunner.Run<SPTreeBenchmark>();
               
            });

            return Ok("Benchmark running in the background");
        }       

    }


}
