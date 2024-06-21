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
            var cate = await _categoryRepository.GetCategoryById(categoryId);
            var listChildren = await _categoryRepository.GetAllChildrenCategory(cate.CategoryId, (cate.Level + 1));
            var response = new Result<List<Category>>();
            if (!listChildren.Any())
            {
                response.Messages = new[] { "This is the final category" };
                response.ResultStatus = ResultStatus.NotFound;
                return response;
            }

            response.Data = listChildren;
            response.Messages = new[] { "List children categories with " + listChildren.Count() };
            response.ResultStatus = ResultStatus.Success;
            return response;
        }

        public async Task<Result<Category>> CreateCategory(Guid parentId, CategoryRequest request)
        {
            try
            {
                var newcate = new Category();
                var response = new Result<Category>();
                var parentCate = await _categoryRepository.GetCategoryById(parentId);
                switch (parentCate.Level)
                {
                    case 1:
                        newcate.Name = request.Name;
                        newcate.Level = 2;
                        newcate.ParentId = parentId;
                        newcate.Status = CategoryStatus.Unavailable;
                        response.Data = await _categoryRepository.AddCategory(newcate);
                        response.Messages = new[] { "Add successfully! Please continue create until the final" };
                        response.ResultStatus = ResultStatus.Success;
                        return response;
                    case 2:
                        newcate.Name = request.Name;
                        newcate.Level = 3;
                        newcate.ParentId = parentId;
                        newcate.Status = CategoryStatus.Unavailable;
                        response.Data = await _categoryRepository.AddCategory(newcate);
                        response.Messages = new[] { "Add successfully! Please continue create until the final" };
                        response.ResultStatus = ResultStatus.Success;
                        return response;
                    case 3:
                        newcate.Name = request.Name;
                        newcate.Level = 4;
                        newcate.ParentId = parentId;
                        newcate.Status = CategoryStatus.Available;

                        parentCate.Status = CategoryStatus.Available;
                        await _categoryRepository.UpdateCategory(parentCate);

                        var grandCate = await _categoryRepository.GetParentCategoryById(parentCate.ParentId);
                        grandCate.Status = CategoryStatus.Available;
                        await _categoryRepository.UpdateCategory(grandCate);
                        
                        response.Data = await _categoryRepository.AddCategory(newcate);
                        response.Messages = new[] { "Add successfully! This is the final one" };
                        response.ResultStatus = ResultStatus.Success;
                        return response;
                }

                response.ResultStatus = ResultStatus.Error;
                response.Messages = new[] { "Error" };
                return response;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<Result<List<Category>>> GetAllParentCategory()
        {
            try
            {
                var response = new Result<List<Category>>();
                var listCate = await _categoryRepository.GetAllParentCategory();
                if (listCate == null)
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
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}