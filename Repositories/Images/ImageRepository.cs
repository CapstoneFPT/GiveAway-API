using BusinessObjects.Entities;
using Dao;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Images
{
    public class ImageRepository : IImageRepository
    {
        

     

        public async Task AddImage(Image image)
        {
            await GenericDao<Image>.Instance.AddAsync(image);
        }

        public async Task AddRangeImage(List<Image> images)
        {
            await GenericDao<Image>.Instance.AddRange(images);
        }
    }
}
