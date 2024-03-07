using Microsoft.AspNetCore.Mvc;
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
        public async Task<IActionResult> UploadBPMN([FromForm] IFormFile file)
        {
            // Check if the file is present
            if (file == null || file.Length == 0)
                return BadRequest("Please upload a file.");

            // Check for the correct file extension (.bpmn)
            if (!Path.GetExtension(file.FileName).Equals(".bpmn", StringComparison.OrdinalIgnoreCase))
                return BadRequest("Only .bpmn files are allowed.");

            XmlDocument bpmnDocument = new XmlDocument();
            try
            {
                // Load the uploaded file into XmlDocument
                using (var stream = file.OpenReadStream())
                {
                    bpmnDocument.Load(stream);
                }

                // Optional: Process the XmlDocument here (e.g., read, validate, or extract data)

                // Respond with success (and any additional info you wish to return)
                return Ok(new { message = "BPMN file loaded successfully." });
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
    }
}
