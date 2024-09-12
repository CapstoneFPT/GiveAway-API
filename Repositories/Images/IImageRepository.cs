using BusinessObjects.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Images
{
    public interface IImageRepository
    {
        Task AddImage(Image image);
        Task AddRangeImage(List<Image> images);
        Task<Image?> GetImageById(Guid imageId);
        Task UpdateSingleImage(Image image);
        void ClearImages(ICollection<Image> images);
    }
}
