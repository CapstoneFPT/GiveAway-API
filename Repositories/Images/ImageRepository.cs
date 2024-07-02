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
        private readonly GenericDao<Image> _imageDao;

        public ImageRepository(GenericDao<Image> imageDao)
        {
            _imageDao = imageDao;
        }

        public async Task AddImage(Image image)
        {
            await _imageDao.AddAsync(image);
        }
    }
}
