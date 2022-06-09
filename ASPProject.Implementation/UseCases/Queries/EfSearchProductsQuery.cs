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
    public class EfSearchProductsQuery : ISearhProductsQuery
    {
        private readonly ProjekatContext _context;
        private readonly IMapper _mapper;

        public EfSearchProductsQuery(ProjekatContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public int Id => 6;

        public string Name => "Get products with search and server-side pagination.";

        public PagedResponse<ProductDto> Execute(ProductsSearch request)
        {
            var query = _context.Products
                        .Include(x => x.Category)
                        .Include(x => x.Prices)
                        .Include(x => x.Specifications)
                        .ThenInclude(x => x.Specification)
                        .Include(x => x.Images)
                        .AsQueryable();

            if (!string.IsNullOrEmpty(request.Keyword))
            {
                query = query.Where(x => x.ProductName.ToLower().Contains(request.Keyword.ToLower()));
            }
            if(request.PerPage == null || request.PerPage < 1)
            {
                request.PerPage = 15;
            }
            if(request.Page == null || request.Page < 1)
            {
                request.Page = 1;
            }
            if (request.Category != 0 || request.Category > 1)
            {
                query = query.Where(x => x.CategoryId == request.Category);
            }

            var toSkip = (request.Page.Value - 1) * request.PerPage.Value;



            var response = new PagedResponse<ProductDto>();
            response.TotalCount = query.Where(x => x.IsActive).Count();
            response.Data = query.Where(x => x.IsActive)
                            .Skip(toSkip)
                            .Take(request.PerPage.Value)
                            .Select(x => _mapper.Map<Product, ProductDto>(x))
                            .ToList();
            response.ItemsPerPage = request.PerPage.Value;
            response.CurrentPage = request.Page.Value;

            return response;

        }
    }
}
