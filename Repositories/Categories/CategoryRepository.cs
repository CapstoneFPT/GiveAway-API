using BusinessObjects.Entities;
using Dao;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            var listcate = await _categoryDao.GetQueryable().Where(c => c.ParentId == null).ToListAsync();
            return listcate;
        }
    }
}
