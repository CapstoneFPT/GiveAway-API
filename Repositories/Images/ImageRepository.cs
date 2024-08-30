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
        

        public async Task UpdateSingleImage(Image image)
        {
            await GenericDao<Image>.Instance.UpdateAsync(image);
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
