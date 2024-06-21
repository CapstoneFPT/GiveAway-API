using BusinessObjects.Entities;
using Dao;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Dtos.Commons;

namespace Repositories.Categories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly GenericDao<Category> _categoryDao;

        public CategoryRepository(GenericDao<Category> categoryDao)
        {
            _categoryDao = categoryDao;
        }

        public async Task<List<Category>> GetAllParentCategory()
        {
            var listcate = await _categoryDao.GetQueryable().Where(c => c.Level == 1).ToListAsync();
            return listcate;
        }

        public async Task<Category> GetCategoryById(Guid id)
        {
            var cate = await _categoryDao.GetQueryable().FirstOrDefaultAsync(c => c.CategoryId == id);
            return cate;
        }

        public async Task<List<Category>> GetAllChildrenCategory(Guid id, int level)
        {
            return await _categoryDao.GetQueryable().Where(c => c.ParentId == id && c.Level == level && c.Status.Equals(CategoryStatus.Available.ToString())).ToListAsync();
        }

        public async Task<Category> AddCategory(Category category)
        {
            return await _categoryDao.AddAsync(category);
        }

        public async Task<Category> UpdateCategory(Category category)
        {
            return await _categoryDao.UpdateAsync(category);
        }

        public async Task<Category> GetParentCategoryById(Guid? id)
        {
            return await _categoryDao.GetQueryable().FirstOrDefaultAsync(c => c.ParentId == id);
        }
    }
}
