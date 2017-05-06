using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ImageSuperResolution.Common;
using ImageSuperResolution.Web.Servicies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace ImageSuperResolution.Web.Controllers.Api
{
    public class UpscallingController : Controller
    {
        private readonly IUpscallingService _upscallingService;

        public UpscallingController(IUpscallingService upscallingService)
        {
            _upscallingService = upscallingService;
        }
        [HttpGet("{ticket}")]
        public string GetProgress(Guid ticket)
        {
            var progressMessage = _upscallingService.GetProgress(ticket);
            return progressMessage.Message;
        }

        [HttpPost]
        public Guid Upload(IFormFile image)
        {
            byte[] imageBytes = ImageUtils.ReadToEnd(image.OpenReadStream());
            return _upscallingService.SendFile(imageBytes);
        }

        
    }
}
