using BusinessObjects.Dtos.Commons;
using BusinessObjects.Entities;
using Microsoft.VisualBasic;
using Repositories.Categories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Categories
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryService(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public Task<Result<List<Category>>> GetAllChildrenCategory()
        {
            throw new NotImplementedException();
        }

        public async Task<Result<List<Category>>> GetAllParentCategory()
        {
            try
            {
                var response = new Result<List<Category>>();
                var listCate = await _categoryRepository.GetAllParentCategory();
                if(listCate == null)
                {
                    response.ResultStatus = ResultStatus.Empty;
                    response.Messages = ["Empty"];
                    return response;
                }
                response.Data = listCate;
                response.ResultStatus = ResultStatus.Success;
                response.Messages = ["successfully"];
                return response;
            }catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
