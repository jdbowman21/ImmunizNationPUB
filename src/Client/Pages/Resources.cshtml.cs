using ImmunizNation.Client.Models;
using ImmunizNation.Client.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using System.Collections.Generic;

namespace ImmunizNation.Client.Pages
{
    [Authorize]
    public class ResourcesModel : PageModel
    {
        private readonly IResourceService _service;

        public ICollection<Resource> Resources;

        public ResourcesModel(IResourceService service)
        {
            _service = service;
        }

        public void OnGet()
        {
            Resources = _service.GetResources();
        }
    }
}
