using BusinessObjects.Dtos.Commons;
using BusinessObjects.Entities;
using Microsoft.VisualBasic;
using Repositories.Categories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Dtos.Category;
using BusinessObjects.Utils;
using DotNext;
using LinqKit;
using Quartz.Util;

namespace Services.Categories
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryService(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<BusinessObjects.Dtos.Commons.Result<List<Category>>> GetAllChildrenCategory(Guid categoryId)
        {
            var response = new BusinessObjects.Dtos.Commons.Result<List<Category>>();
            var cate = await _categoryRepository.GetCategoryById(categoryId);
            if (cate.Status == CategoryStatus.Unavailable)
            {
                response.Messages = new[] { "This is an unavailable category" };
                response.ResultStatus = ResultStatus.NotFound;
                return response;
            }

            var listChildren = await _categoryRepository.GetAllChildrenCategory(cate.CategoryId, (cate.Level + 1));

            if (!listChildren.Any())
            {
                response.Messages = new[] { "This is the final category" };
                response.ResultStatus = ResultStatus.NotFound;
                return response;
            }

            response.Data = listChildren;
            response.Messages = new[] { "List children categories with " + listChildren.Count };
            response.ResultStatus = ResultStatus.Success;
            return response;
        }

        public async Task<BusinessObjects.Dtos.Commons.Result<Category>> CreateCategory(Guid parentId, CreateCategoryRequest request)
        {
            var newCategory = new Category();
            var response = new BusinessObjects.Dtos.Commons.Result<Category>();
            var parentCate = await _categoryRepository.GetCategoryById(parentId);
            switch (parentCate.Level)
            {
                case 1:
                    newCategory.Name = request.Name;
                    newCategory.Level = 2;
                    newCategory.ParentId = parentId;
                    newCategory.Status = parentCate.Status;
                    newCategory.CreatedDate = DateTime.UtcNow;
                    response.Data = await _categoryRepository.AddCategory(newCategory);
                    response.Messages = new[] { "Add successfully! Please continue create until the final" };
                    response.ResultStatus = ResultStatus.Success;
                    return response;
                case 2:
                    newCategory.Name = request.Name;
                    newCategory.Level = 3;
                    newCategory.ParentId = parentId;
                    newCategory.Status = parentCate.Status;
                    newCategory.CreatedDate = DateTime.UtcNow;
                    response.Data = await _categoryRepository.AddCategory(newCategory);
                    response.Messages = new[] { "Add successfully! Please continue create until the final" };
                    response.ResultStatus = ResultStatus.Success;
                    return response;
                case 3:
                    newCategory.Name = request.Name;
                    newCategory.Level = 4;
                    newCategory.ParentId = parentId;
                    newCategory.Status = parentCate.Status;
                    newCategory.CreatedDate = DateTime.UtcNow;
                    
                    await _categoryRepository.UpdateCategory(parentCate);

                    response.Data = await _categoryRepository.AddCategory(newCategory);
                    response.Messages = new[] { "Add successfully! This is the final one" };
                    response.ResultStatus = ResultStatus.Success;
                    return response;
            }

            response.ResultStatus = ResultStatus.Error;
            response.Messages = new[] { "Error" };
            return response;
        }

        public async Task<List<CategoryTreeNode>> GetTree(Guid? shopId = null, Guid? rootCategoryId = null, bool? isAvailable = null)
        {
            var result = await _categoryRepository.GetCategoryTree(shopId,rootCategoryId, isAvailable);
            return result;
        }

        public async Task<BusinessObjects.Dtos.Commons.Result<List<Category>>> GetAllParentCategory()
        {
            var response = new BusinessObjects.Dtos.Commons.Result<List<Category>>();
            var listCate = await _categoryRepository.GetAllParentCategory();
            if (listCate.Count == 0)
            {
                response.ResultStatus = ResultStatus.Success;
                response.Messages = ["Empty"];
                return response;
            }

            response.Data = listCate;
            response.ResultStatus = ResultStatus.Success;
            response.Messages = ["successfully"];
            return response;
        }

        public async Task<BusinessObjects.Dtos.Commons.Result<List<Category>>> GetCategoryWithCondition(CategoryRequest categoryRequest)
        {
            var response = new BusinessObjects.Dtos.Commons.Result<List<Category>>();
            var listCate = await _categoryRepository.GetCategoryWithCondition(categoryRequest);
            if (categoryRequest.Level > 4 || categoryRequest.Level < 1)
            {
                response.ResultStatus = ResultStatus.Error;
                response.Messages = ["Level is only from 1 to 4"];
                return response;
            }
            if (listCate.Count == 0)
            {
                response.ResultStatus = ResultStatus.Success;
                response.Messages = ["Empty"];
                return response;
            }
            response.Data = listCate;
            response.ResultStatus = ResultStatus.Success;
            response.Messages = ["Result with " + listCate.Count + " categories"];
            return response;
        }

        

        public async Task<BusinessObjects.Dtos.Commons.Result<CategoryResponse>> UpdateNameCategory(Guid categoryId, UpdateCategoryRequest request)
        {
            var category = await _categoryRepository.GetCategoryById(categoryId);
            if (category is null)
            {
                throw new CategoryNotFound("Can not find category");
            }

            if (request.Name.IsNullOrWhiteSpace())
            {
                throw new MissingFeatureException("Can not replace by whit space");
            }
            category.Name = request.Name;
            await _categoryRepository.UpdateCategory(category);
            return new BusinessObjects.Dtos.Commons.Result<CategoryResponse>()
            {
                Data = new CategoryResponse()
                {
                    CategoryId = category.CategoryId,
                    Level = category.Level,
                    Name = category.Name,
                    Status = category.Status
                },
                ResultStatus = ResultStatus.Success,
                Messages = new []{"Update successfully"}
            };
        }

        public async Task<BusinessObjects.Dtos.Commons.Result<CategoryResponse>> UpdateStatusCategory(Guid categoryId)
        {
            var category = await _categoryRepository.GetCategoryById(categoryId);
            if (category is null)
            {
                throw new CategoryNotFound("Can not find category");
            }

            if (category.Status == CategoryStatus.Available)
            {
                category = await _categoryRepository.UpdateStatusCategory(categoryId, CategoryStatus.Unavailable);
            }
            else
            {
                category = await _categoryRepository.UpdateStatusCategory(categoryId, CategoryStatus.Available);
            }

            return new BusinessObjects.Dtos.Commons.Result<CategoryResponse>()
            {
                Data = new CategoryResponse()
                {
                    CategoryId = category.CategoryId,
                    Level = category.Level,
                    Name = category.Name,
                    Status = category.Status
                },
                ResultStatus = ResultStatus.Success,
                Messages = new []{"Successfully"}
            };
        }
    }
}