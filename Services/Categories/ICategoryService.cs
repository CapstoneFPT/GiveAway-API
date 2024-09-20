using BusinessObjects.Dtos.Commons;
using BusinessObjects.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Dtos.Category;
using Microsoft.AspNetCore.Mvc;

namespace Services.Categories
{
    public interface ICategoryService
    {
        Task<Result<List<Category>>> GetAllParentCategory();
        Task<Result<List<Category>>> GetAllChildrenCategory(Guid categoryId);
        Task<Result<Category>> CreateCategory(Guid parentId, CreateCategoryRequest request);
        Task<List<CategoryTreeNode>> GetTree(Guid? shopId = null, Guid? rootCategoryId = null, bool? isAvailable = null);
        Task<Result<List<Category>>> GetCategoryWithCondition(CategoryRequest categoryRequest);
        
        Task<Result<CategoryResponse>> UpdateNameCategory(Guid categoryId, UpdateCategoryRequest request);
        Task<Result<CategoryResponse>> UpdateStatusCategory(Guid categoryId);
    }
}
