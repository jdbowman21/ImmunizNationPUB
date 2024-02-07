using ImmunizNation.Client.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using ImmunizNation.Client.Models;
using System.Linq;
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ImmunizNation.Client.Services
{

    public class ResourceDownloadViewModel
    {
        public Resource Resource { get; set; }

        public List<ResourceDownloadCountViewModel> UserDownloads { get; set; }
    }

    public class ResourceDownloadCountViewModel
    {
        public ApplicationUser User { get; set; }
        public int Count { get; set; }
    }

    public interface IResourceService
    {
        ICollection<Resource> GetResources();
        Task<Resource> GetResourceById(Guid resourceId, ApplicationUser user);
        Task<ICollection<Resource>> GetAllResourcesAsync();
        Task<ICollection<ResourceDownloadViewModel>> GetResourceDownloadsCounts();
        Task<ICollection<ResourceDownload>> GetResourceDownloads();
    }

    public class ResourceService : IResourceService
    {

        private readonly ApplicationDbContext _context;

        public ResourceService(ApplicationDbContext context)
        {
            _context = context;
        }

        public ICollection<Resource> GetResources()
        {
            return _context.Resources.OrderBy(r=>r.Order).ToArray();
        }

        public async Task<ICollection<ResourceDownload>> GetResourceDownloads()
        {
            return await _context.ResourceDownloads
                .Include(r=>r.Resource)
                .Include(r=>r.User)
                .OrderByDescending(r => r.UserId)
                .ThenBy(r => r.Resource.Order)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<ICollection<ResourceDownloadViewModel>> GetResourceDownloadsCounts()
        {
            var resources = await _context.Resources.ToArrayAsync();

            List<ResourceDownloadViewModel> resourceDownloads = new List<ResourceDownloadViewModel>();

            foreach (var resource in resources)
            {
               var downloads = await _context.ResourceDownloads
                    .Include(r => r.User)
                    .Where(r => r.ResourceId == resource.Id).Select(r => new ResourceDownloadCountViewModel
                    {
                        Count = r.Count,
                        User = r.User
                    })
                    .ToListAsync();

                resourceDownloads.Add(new ResourceDownloadViewModel
                {
                    Resource = resource,
                    UserDownloads = downloads
                });
            }

            return resourceDownloads;
        }

        public async Task<Resource> GetResourceById(Guid resourceId, ApplicationUser user)
        {
            Resource resource = _context.Resources
                .Where(r => r.Id == resourceId)
                .FirstOrDefault();

            if(resource == null || user == null)
            {
                return null;
            }

            // keep track of the user downloads.
            ResourceDownload foundResourceDownload = _context.ResourceDownloads.Where(r => r.ResourceId == resourceId && r.UserId == user.Id).FirstOrDefault();
            if(foundResourceDownload == null)
            {
                _context.Add(new ResourceDownload
                {
                    Resource = resource,
                    User = user,
                    Count = 1
                });
            } else
            {
                foundResourceDownload.Count++;
            }

            await _context.SaveChangesAsync();

            return resource;

        }

        public async Task<ICollection<Resource>> GetAllResourcesAsync()
        {
            return await _context.Resources
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
