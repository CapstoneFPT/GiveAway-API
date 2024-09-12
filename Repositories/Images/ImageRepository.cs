using BusinessObjects.Entities;
using Dao;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Repositories.Images
{
    public class ImageRepository : IImageRepository
    {
        
        private readonly GiveAwayDbContext _giveAwayDbContext;

        public ImageRepository(GiveAwayDbContext giveAwayDbContext)
        {
            _giveAwayDbContext = giveAwayDbContext;
        }

        public async Task UpdateSingleImage(Image image)
        {
            await GenericDao<Image>.Instance.UpdateAsync(image);
        }

        public void ClearImages(ICollection<Image> images)
        {
             _giveAwayDbContext.Images 
                .RemoveRange(images);
            _giveAwayDbContext.SaveChanges();
        }

        public async Task AddImage(Image image)
        {
            await GenericDao<Image>.Instance.AddAsync(image);
        }

        public async Task AddRangeImage(List<Image> images)
        {
            await GenericDao<Image>.Instance.AddRange(images);
        }

        public async Task<Image?> GetImageById(Guid imageId)
        {
            return await GenericDao<Image>.Instance.GetQueryable().FirstOrDefaultAsync(c => c.ImageId == imageId);
        }

    }
}
