using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ImageSuperResolution.Common;
using ImageSuperResolution.Web.Servicies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ImageSuperResolution.Common.Messages.ViewModels;
using ImageSuperResolution.Common.Messages.QueueEvents;

namespace ImageSuperResolution.Web.Controllers.Api
{
    public class UpscallingController : Controller
    {
        private readonly IUpscallingService _upscallingService;

        public UpscallingController(IUpscallingService upscallingService)
        {
            _upscallingService = upscallingService;
        }

        [HttpGet]
        public IEnumerable<TaskProgress> GetProgress(Guid ticket)
        {
            var progressMessages = _upscallingService.GetProgress(ticket);
            return progressMessages;
        }

        [HttpGet]
        public ResultInfo GetResult(Guid ticket)
        {
            var resultInfo = _upscallingService.GetResultInfo(ticket);
            return resultInfo;
        }

        [HttpPost]
        public bool Clear(Guid ticket)
        {
            return _upscallingService.ClearEvents(ticket);
        }

        [HttpPost]
        public async Task<Guid> Upload(IFormFile image)
        {
            byte[] imageBytes = ImageUtils.ReadToEnd(image.OpenReadStream());
            return await _upscallingService.SendFile(imageBytes);
        }

        
    }
}
