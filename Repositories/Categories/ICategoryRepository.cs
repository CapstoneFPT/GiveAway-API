using BusinessObjects.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Dtos.Category;

namespace Repositories.Categories
{
    public interface ICategoryRepository
    {
        Task<List<Category>> GetAllParentCategory();
        Task<Category> GetCategoryById(Guid id);
        Task<List<Category>> GetAllChildrenCategory(Guid id, int level);
        Task<Category> AddCategory(Category category);
        Task<Category> UpdateCategory(Category category);
        Task<Category> GetParentCategoryById(Guid? id);
        Task<List<CategoryTreeNode>> GetCategoryTree(Guid? shopId = null);
    }
}
