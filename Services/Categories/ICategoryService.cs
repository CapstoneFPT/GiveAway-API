﻿using BusinessObjects.Dtos.Commons;
using BusinessObjects.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Dtos.Category;

namespace Services.Categories
{
    public interface ICategoryService
    {
        Task<Result<List<Category>>> GetAllParentCategory();
        Task<Result<List<Category>>> GetAllChildrenCategory(Guid categoryId);
        Task<Result<Category>> CreateCategory(Guid parentId, CategoryRequest request);
    }
}