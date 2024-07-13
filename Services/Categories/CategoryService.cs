using BusinessObjects.Dtos.Commons;
using BusinessObjects.Entities;
using Microsoft.VisualBasic;
using Repositories.Categories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Dtos.Category;

namespace Services.Categories
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryService(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<Result<List<Category>>> GetAllChildrenCategory(Guid categoryId)
        {
            var response = new Result<List<Category>>();
            var cate = await _categoryRepository.GetCategoryById(categoryId);
            if (cate.Status.Equals("Unavailable"))
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

        public async Task<Result<Category>> CreateCategory(Guid parentId, CreateCategoryRequest request)
        {
            var newCategory = new Category();
            var response = new Result<Category>();
            var parentCate = await _categoryRepository.GetCategoryById(parentId);
            switch (parentCate.Level)
            {
                case 1:
                    newCategory.Name = request.Name;
                    newCategory.Level = 2;
                    newCategory.ParentId = parentId;
                    newCategory.Status = CategoryStatus.Unavailable;
                    response.Data = await _categoryRepository.AddCategory(newCategory);
                    response.Messages = new[] { "Add successfully! Please continue create until the final" };
                    response.ResultStatus = ResultStatus.Success;
                    return response;
                case 2:
                    newCategory.Name = request.Name;
                    newCategory.Level = 3;
                    newCategory.ParentId = parentId;
                    newCategory.Status = CategoryStatus.Unavailable;
                    response.Data = await _categoryRepository.AddCategory(newCategory);
                    response.Messages = new[] { "Add successfully! Please continue create until the final" };
                    response.ResultStatus = ResultStatus.Success;
                    return response;
                case 3:
                    newCategory.Name = request.Name;
                    newCategory.Level = 4;
                    newCategory.ParentId = parentId;
                    newCategory.Status = CategoryStatus.Available;

                    parentCate.Status = CategoryStatus.Available;
                    await _categoryRepository.UpdateCategory(parentCate);

                    var grandCate = await _categoryRepository.GetParentCategoryById(parentCate.ParentId);
                    grandCate.Status = CategoryStatus.Available;
                    await _categoryRepository.UpdateCategory(grandCate);

                    response.Data = await _categoryRepository.AddCategory(newCategory);
                    response.Messages = new[] { "Add successfully! This is the final one" };
                    response.ResultStatus = ResultStatus.Success;
                    return response;
            }

            response.ResultStatus = ResultStatus.Error;
            response.Messages = new[] { "Error" };
            return response;
        }

        public async Task<List<CategoryTreeNode>> GetTree(Guid? shopId = null)
        {
            var result = await _categoryRepository.GetCategoryTree(shopId);
            return result;
        }

        public async Task<Result<List<Category>>> GetAllParentCategory()
        {
            var response = new Result<List<Category>>();
            var listCate = await _categoryRepository.GetAllParentCategory();
            if (listCate.Count == 0)
            {
                response.ResultStatus = ResultStatus.Empty;
                response.Messages = ["Empty"];
                return response;
            }

            response.Data = listCate;
            response.ResultStatus = ResultStatus.Success;
            response.Messages = ["successfully"];
            return response;
        }

        public async Task<Result<List<Category>>> GetCategoryWithCondition(CategoryRequest categoryRequest)
        {
            var response = new Result<List<Category>>();
            var listCate = await _categoryRepository.GetCategoryWithCondition(categoryRequest);
            if (categoryRequest.Level > 4 || categoryRequest.Level < 1)
            {
                response.ResultStatus = ResultStatus.Error;
                response.Messages = ["Level is only from 1 to 4"];
                return response;
            }
            if (listCate.Count == 0)
            {
                response.ResultStatus = ResultStatus.Empty;
                response.Messages = ["Empty"];
                return response;
            }
            response.Data = listCate;
            response.ResultStatus = ResultStatus.Success;
            response.Messages = ["Result with " + listCate.Count + " categories"];
            return response;
        }
    }
}