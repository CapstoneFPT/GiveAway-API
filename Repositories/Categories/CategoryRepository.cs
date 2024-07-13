using BusinessObjects.Entities;
using Dao;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Dtos.Category;
using BusinessObjects.Dtos.Commons;
using Org.BouncyCastle.Asn1.Ocsp;

namespace Repositories.Categories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly GenericDao<Category> _categoryDao;
        private readonly GenericDao<FashionItem> _fashionItemDao;

        public CategoryRepository(GenericDao<Category> categoryDao, GenericDao<FashionItem> fashionItemDao)
        {
            _categoryDao = categoryDao;
            _fashionItemDao = fashionItemDao;
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
            return await _categoryDao.GetQueryable()
                .Where(c => c.ParentId == id && c.Level == level && c.Status.Equals(CategoryStatus.Available))
                .ToListAsync();
        }

        public async Task<Category> AddCategory(Category category)
        {
            return await _categoryDao.AddAsync(category);
        }

        public async Task<List<CategoryTreeNode>> GetCategoryTree(Guid? shopId = null)
        {
            IQueryable<Guid> relevantCategoryIds;

            if (shopId.HasValue)
            {
                relevantCategoryIds = _fashionItemDao.GetQueryable()
                    .Where(fi => fi.ShopId == shopId.Value)
                    .Select(fi => fi.CategoryId)
                    .Distinct();
            }
            else
            {
                relevantCategoryIds = _categoryDao.GetQueryable().Select(c => c.CategoryId);
            }

            var allCategories = await _categoryDao.GetQueryable()
                .Where(c => relevantCategoryIds.Contains(c.CategoryId))
                .Select(c => new CategoryTreeNode
                {
                    CategoryId = c.CategoryId,
                    ParentId = c.ParentId,
                    Level = c.Level,
                    Name = c.Name
                })
                .ToListAsync();

            var rootCategories = allCategories.Where(c => c.ParentId == null).ToList();
            foreach (var rootCategory in rootCategories)
            {
                BuildCategoryTree(rootCategory, allCategories);
            }

            return rootCategories;
        }

        private void BuildCategoryTree(CategoryTreeNode parent, List<CategoryTreeNode> allCategories)
        {
            parent.Children = allCategories.Where(c => c.ParentId == parent.CategoryId).ToList();
            foreach (var child in parent.Children)
            {
                BuildCategoryTree(child, allCategories);
            }
        }

        public async Task<Category> UpdateCategory(Category category)
        {
            return await _categoryDao.UpdateAsync(category);
        }

        public async Task<Category> GetParentCategoryById(Guid? id)
        {
            return await _categoryDao.GetQueryable().FirstOrDefaultAsync(c => c.ParentId == id);
        }

        public async Task<List<Category>> GetCategoryWithCondition(CategoryRequest categoryRequest)
        {
            var query = _categoryDao.GetQueryable();
            if (!string.IsNullOrWhiteSpace(categoryRequest.SearchName))
                query = query.Where(x => EF.Functions.ILike(x.Name, $"%{categoryRequest.SearchName}%"));
            if (categoryRequest.Level != null)
            {
                query = query.Where(f => f.Level.Equals(categoryRequest.Level));
            }
            if (categoryRequest.CategoryId != null)
            {
                query = query.Where(f => f.CategoryId.Equals(categoryRequest.CategoryId));
            }
            if (categoryRequest.Status != null)
            {
                query = query.Where(f => f.Status.Equals(categoryRequest.Status));
            }
            var items = await query.AsNoTracking().ToListAsync();
            return items;
        }
    }
}