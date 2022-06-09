using ASPProject.Application.DTOs;
using ASPProject.Application.DTOs.Searches;
using ASPProject.Application.UseCases.Queries;
using ASPProject.Domain;
using ASPProjekat.DataAccess;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASPProject.Implementation.UseCases.Queries
{
    public class EfGetCategoriesQuery : IGetCategoriesQuery
    {
        private readonly IMapper _mapper;
        private readonly ProjekatContext _context;

        public EfGetCategoriesQuery(IMapper mapper, ProjekatContext context)
        {
            _mapper = mapper;
            _context = context;
        }

        public int Id => 16;

        public string Name => "Search categories";

        public PagedResponse<CategoryDto> Execute(BasePagedSearch request)
        {
            var query = _context.Categories.Include(x => x.ChildCategories)
                        .Include(x => x.ParentCategory).AsQueryable();

            if (!string.IsNullOrEmpty(request.Keyword))
            {
                query = query.Where(x => x.CategoryName.ToLower().Contains(request.Keyword.ToLower()));
            }
            if (request.PerPage == null || request.PerPage < 1)
            {
                request.PerPage = 15;
            }

            if (request.Page == null || request.Page < 1)
            {
                request.Page = 1;
            }

            var toSkip = (request.Page.Value - 1) * request.PerPage.Value;



            var response = new PagedResponse<CategoryDto>();
            response.TotalCount = query.Count();
            response.Data = query.Where(x => x.IsActive)
                            .Skip(toSkip)
                            .Take(request.PerPage.Value)
                            .Select(x => _mapper.Map<Category, CategoryDto>(x)).ToList();
            response.ItemsPerPage = request.PerPage.Value;
            response.CurrentPage = request.Page.Value;

            return response;
        }
    }
}
