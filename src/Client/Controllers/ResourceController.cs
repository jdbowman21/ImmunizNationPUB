using ImmunizNation.Client.Models;
using ImmunizNation.Client.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ImmunizNation.Client.Controllers
{
    [Authorize]
    public class ResourceController : Controller
    {
        private readonly IServiceProvider _services;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IResourceService _resourceService;

        public ResourceController(IServiceProvider services,
            UserManager<ApplicationUser> userManager,
            IResourceService resourceService)
        {
            _services = services;
            _userManager = userManager;
            _resourceService = resourceService;
        }

        [HttpPost]
        public async Task<IActionResult> GetResource(string resourceId)
        {
            // get the document by the resourceId
            ApplicationUser user = await _userManager.GetUserAsync(User);
            Resource resouce = await _resourceService.GetResourceById(Guid.Parse(resourceId), user);
            if(resouce == null)
            {
                return NotFound();
            }

            var env = _services.GetService<IWebHostEnvironment>();
            var path = env.ContentRootPath + "\\" + resouce.Path;

            byte[] bytes = System.IO.File.ReadAllBytes(path);

            return File(
                bytes,
                resouce.MimeType,
                resouce.FileName);
        }
    }
}
