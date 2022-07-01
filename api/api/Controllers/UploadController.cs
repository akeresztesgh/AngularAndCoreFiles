using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Route("[controller]")]
    public class UploadController : ControllerBase
    {
        private const string UploadPath = "./uploads";
        private readonly IFlowUploadProcessorCore flowUploadProcessorCore;

        public UploadController(IFlowUploadProcessorCore flowUploadProcessorCore)
        {
            this.flowUploadProcessorCore = flowUploadProcessorCore;
        }

        [Route(""), HttpPost]
        public async Task<IActionResult> UploadFilePart([System.Web.Http.FromUri] FlowMetaData filePart)
        {
            if (!Request.HasFormContentType || !Request.Form.Files.Any())
            {
                return BadRequest();
            }

            await flowUploadProcessorCore.ProcessUploadChunkRequest(Request, filePart, UploadPath);
            return Ok();
        }

        [HttpGet]
        public IActionResult Get([System.Web.Http.FromUri] FlowMetaData flowMeta)
        {
            if (flowUploadProcessorCore.HasRecievedChunk(flowMeta))
            {
                return Ok();
            }

            return NoContent();
        }

    }
}
