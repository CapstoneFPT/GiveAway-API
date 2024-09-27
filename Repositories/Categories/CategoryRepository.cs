using BusinessObjects.Entities;
using Dao;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Dtos.Category;
using BusinessObjects.Dtos.Commons;
using Org.BouncyCastle.Asn1.Ocsp;

namespace Repositories.Categories
{
    public class CategoryMiniNode
    {
        public Guid CategoryId { get; set; }
        public Guid? ParentId { get; set; }
    }

    public class CategoryRepository : ICategoryRepository
    {
        private readonly GiveAwayDbContext _giveAwayDbContext;

        public CategoryRepository(GiveAwayDbContext giveAwayDbContext)
        {
            _giveAwayDbContext = giveAwayDbContext;
        }

        public async Task<List<Category>> GetAllParentCategory()
        {
            var listcate = await GenericDao<Category>.Instance.GetQueryable().Where(c => c.Level == 1).ToListAsync();
            return listcate;
        }

        public async Task<Category> GetCategoryById(Guid id)
        {
            var cate = await GenericDao<Category>.Instance.GetQueryable().FirstOrDefaultAsync(c => c.CategoryId == id);
            return cate;
        }

        public async Task<Category> UpdateStatusCategory(Guid categoryId, CategoryStatus status)
        {
            var category = await GenericDao<Category>.Instance.GetQueryable()
                
                .FirstOrDefaultAsync(c => c.CategoryId == categoryId);
            var listChildrenIds = await GetAllChildrenCategoryIds(categoryId);
            if (listChildrenIds.Count > 0)
            {
                foreach (var childrenId in listChildrenIds)
                {
                    var childrenCategory = await GenericDao<Category>.Instance.GetQueryable().FirstOrDefaultAsync(c => c.CategoryId == childrenId);
                    childrenCategory.Status = status;
                    await GenericDao<Category>.Instance.UpdateAsync(childrenCategory);
                }
            }

            category.Status = status;
            await GenericDao<Category>.Instance.UpdateAsync(category);
            return category;
        }

        public async Task<List<Category>> GetAllChildrenCategory(Guid id, int level)
        {
            return await GenericDao<Category>.Instance.GetQueryable()
                .Where(c => c.ParentId == id && c.Level == level && c.Status.Equals(CategoryStatus.Available))
                .ToListAsync();
        }

        public async Task<List<Guid>> GetAllChildrenCategoryIds(Guid categoryId)
        {
            var allCategories = await _giveAwayDbContext.Categories
                .Select(c => new CategoryMiniNode { CategoryId = c.CategoryId, ParentId = c.ParentId })
                .ToListAsync();

            var childrenIds = new HashSet<Guid>();
            GetChildrenIdsRecursive(categoryId, allCategories, childrenIds);
            childrenIds.Add(categoryId);
            return childrenIds.ToList();
        }

        private void GetChildrenIdsRecursive(Guid parentId, List<CategoryMiniNode> allCategories,
            HashSet<Guid> childrenIds)
        {
            var children = allCategories
                .Where(c => c.ParentId == parentId)
                .Select(c => c.CategoryId)
                .Where(childrenIds.Add)
                .ToList();

            children.ForEach(childId => 
                GetChildrenIdsRecursive(childId, allCategories, childrenIds));
        }

        public async Task<Category> AddCategory(Category category)
        {
            return await GenericDao<Category>.Instance.AddAsync(category);
        }

        public async Task<List<CategoryTreeNode>> GetCategoryTree(Guid? shopId = null, Guid? rootCategoryId = null, bool? isAvailable = null)
        {
            IQueryable<Guid> relevantCategoryIds;
            var categoryDao = _giveAwayDbContext.Set<Category>();
            var fashionItemDao = _giveAwayDbContext.Set<IndividualFashionItem>();

            if (shopId.HasValue)
            {
                relevantCategoryIds = fashionItemDao
                    // .Where(fi => fi.ShopId == shopId.Value)
                    .Select(fi => fi.MasterItem.CategoryId)
                    .Distinct();
            }
            else
            {
                relevantCategoryIds = categoryDao.Select(c => c.CategoryId);
            }

            if (isAvailable is true)
            {
                relevantCategoryIds = categoryDao.Where(c => c.Status == CategoryStatus.Available).Select(c => c.CategoryId);
            }else if (isAvailable is false)
            {
                relevantCategoryIds = categoryDao.Where(c => c.Status == CategoryStatus.Unavailable).Select(c => c.CategoryId);
            }
            var allCategories = await categoryDao
                .Where(c => relevantCategoryIds.Contains(c.CategoryId))
                .Select(c => new CategoryTreeNode
                {
                    CategoryId = c.CategoryId,
                    ParentId = c.ParentId,
                    Level = c.Level,
                    Name = c.Name,
                    Status = c.Status
                })
                .ToListAsync();

            List<CategoryTreeNode> rootCategories;

            if (rootCategoryId.HasValue)
            {
                rootCategories = allCategories.Where(c => c.CategoryId == rootCategoryId.Value).ToList();
                if (!rootCategories.Any())
                    return new List<CategoryTreeNode>();
            }
            else
            {
                rootCategories = allCategories.Where(c => c.ParentId == null).ToList();
            }

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
            return await GenericDao<Category>.Instance.UpdateAsync(category);
        }

        public async Task<Category> GetParentCategoryById(Guid? id)
        {
            return await GenericDao<Category>.Instance.GetQueryable().FirstOrDefaultAsync(c => c.ParentId == id);
        }

        public async Task<List<Category>> GetCategoryWithCondition(CategoryRequest categoryRequest)
        {
            var query = GenericDao<Category>.Instance.GetQueryable();
            if (categoryRequest.ParentId != null)
            {
                var lv2 = await GenericDao<Category>.Instance.GetQueryable()
                    .Where(c => c.ParentId == categoryRequest.ParentId).ToListAsync();
                var listCategoryLv3 = new List<Category>();
                foreach (var item in lv2)
                {
                    var lv3 = await GenericDao<Category>.Instance.GetQueryable()
                        .Where(c => c.ParentId == item.CategoryId).ToListAsync();
                    listCategoryLv3.AddRange(lv3);
                }

                var lisCategoryLv4 = new List<Category>();
                foreach (var item in listCategoryLv3)
                {
                    var lv4 = await GenericDao<Category>.Instance.GetQueryable()
                        .Where(c => c.ParentId == item.CategoryId).ToListAsync();
                    lisCategoryLv4.AddRange(lv4);
                }

                if (categoryRequest.Status != null)
                {
                    lisCategoryLv4 = lisCategoryLv4.Where(c => c.Status == categoryRequest.Status).ToList();
                }
                return lisCategoryLv4;
            }

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

        public async Task<CategoryLeavesResponse> GetLeaves(Guid? shopId)
        {
            IQueryable<Category> relevantCategories;
            if (shopId.HasValue)
            {
                relevantCategories = GenericDao<IndividualFashionItem>.Instance.GetQueryable()
                    // .Where(x => x.ShopId == shopId.Value)
                    .Select(x => x.MasterItem.Category).Where(x => x.Level == 4 && x.Status.Equals(CategoryStatus.Available))
                    .Distinct();
            }
            else
            {
                relevantCategories = GenericDao<Category>.Instance.GetQueryable()
                    .Where(x => x.Level == 4 && x.Status.Equals(CategoryStatus.Available)).Distinct();
            }

            var items = await relevantCategories.Select(x => new CategoryTreeNode()
            {
                CategoryId = x.CategoryId,
                ParentId = x.ParentId,
                Level = x.Level,
                Name = x.Name
            }).ToListAsync();

            return new CategoryLeavesResponse()
            {
                ShopId = shopId,
                CategoryLeaves = items
            };
        }
    }
}