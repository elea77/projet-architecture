using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Archi.Library.Extensions;
using Archi.Library.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Archi.Library.Controllers
{
    public abstract class BaseController<TContext, TModel> : ControllerBase where TContext : DbContext where TModel : ModelBase
    {
        protected readonly TContext _context;

        public BaseController(TContext context)
        {
            _context = context;
        }

        protected System.Linq.IQueryable<TModel> GetRanged(string range, IQueryable<TModel> query)
        {
            var parameters = Request.Query.Where((x) => x.Key != "range");
            string urlParams = "";
            if (parameters.Count() > 0)
            {
                foreach (var parameter in parameters)
                {
                    urlParams = urlParams + "&" + parameter.Key + "=" + parameter.Value;
                }
            }

            string[] limits = range.Split('-');
            int limitMin = int.Parse(limits[0]);
            int limitMax = int.Parse(limits[1]);
            int numberElement = int.Parse(limits[1]) - int.Parse(limits[0]) + 1;
            int countElement = query.Count();

            int prevLimitMin = limitMin - numberElement;
            int prevLimitMax = limitMax - numberElement;
            if (prevLimitMin < 0)
            {
                prevLimitMin = 0;
                prevLimitMax = prevLimitMin + numberElement - 1;
            }
            string rangePrev = prevLimitMin.ToString() + "-" + prevLimitMax.ToString();

            int nextLimitMin = limitMin + numberElement;
            int nextLimitMax = limitMax + numberElement;
            if (nextLimitMax > countElement)
            {
                nextLimitMin = countElement - numberElement;
                nextLimitMax = countElement - 1;
            }
            string rangeNext = nextLimitMin.ToString() + "-" + nextLimitMax.ToString();


            Type myType = typeof(TModel);
            string typeName = myType.Name;
            string basePath = Request.Path;
            string firstLink = basePath + "?range=0-" + (numberElement - 1) + urlParams;
            string lastLink = basePath + "?range=" + (countElement - numberElement) + "-" + (countElement - 1) + urlParams;
            string prevLink = basePath + "?range=" + rangePrev + urlParams;
            string nextLink = basePath + "?range=" + rangeNext + urlParams;
            Response.Headers.Add("Content-Range", range + "/" + numberElement);
            Response.Headers.Add("Accept-Range", typeName + " 10");
            Response.Headers.Add("Link", firstLink + "; rel=\"first\", " + prevLink + "; rel=\"prev\", " + nextLink + "; rel=\"next\", " + lastLink + "; rel=\"last\"");

            query = query.Skip(limitMin).Take(numberElement);
            return query;
        }

        protected System.Linq.IQueryable<TModel> OrderBy(string order, string element, IQueryable<TModel> query)
        {
            var parameter = Expression.Parameter(typeof(TModel), "x");

            Expression property = Expression.Property(parameter, element);
            var lambda = Expression.Lambda(property, parameter);

            // REFLECTION: source.OrderBy(x => x.Property)
            var orderByMethod = typeof(Queryable).GetMethods().First(x => x.Name == order && x.GetParameters().Length == 2);
            var orderByGeneric = orderByMethod.MakeGenericMethod(typeof(TModel), property.Type);
            var newQuery = orderByGeneric.Invoke(null, new object[] { query, lambda });

            return ((IOrderedQueryable<TModel>)newQuery);
        }

        protected System.Linq.IQueryable<TModel> SearchByElement(string element, string search, IQueryable<TModel> query)
        {
            var parameter = Expression.Parameter(typeof(TModel), "x");

            Expression property = Expression.Property(parameter, element);
            Expression<Func<TModel, bool>> lambda;

            var splitedSearch = search.Split(",");
            IQueryable<TModel> finalQuery = null;

            for (int i = 0; i < splitedSearch.Count(); i++)
            {
                MethodInfo method = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                var someValue = Expression.Constant(splitedSearch[i], typeof(string));
                var containsMethodExp = Expression.Call(property, method, someValue);
                lambda = Expression.Lambda<Func<TModel, bool>>(containsMethodExp, parameter);
                IQueryable<TModel> newQuery = query.Where(lambda);
                if (finalQuery == null)
                {
                    finalQuery = newQuery;
                }
                else
                {
                    finalQuery = finalQuery.Concat(newQuery);
                }
            }

            return finalQuery;
        }

        // GET: api/Pizzas?range=0-5 | api/Pizzas?asc=price
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TModel>>> Index(string range, string asc, string desc)
        {
            var query = _context.Set<TModel>().Where(x => x.Active == true);

            if (Request != null)
            {
                var parameters = Request.Query.Where((x) => x.Key != "fields" && x.Key != "range" && x.Key != "asc" && x.Key != "desc");

                if (parameters.Count() > 0)
                {
                    foreach (var parameter in parameters)
                    {
                        query = query.filter(parameter.Key, parameter.Value);
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(range))
            {
                query = GetRanged(range, query);
            }

            if (asc != null)
            {
                query = OrderBy("OrderBy", asc, query);
            }

            if (desc != null)
            {
                query = OrderBy("OrderByDescending", desc, query);
            }

            var list = await query.ToListAsync();
            return list;
        }

        // GET: api/Pizzas/search?name=*napoli*&type=pizza
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<TModel>>> Search()
        {
            var query = _context.Set<TModel>().Where(x => x.Active == true);

            if (Request != null)
            {
                var parameters = Request.Query.Where((x) => x.Key != null);
                if (parameters.Count() > 0)
                {
                    foreach (var parameter in parameters)
                    {
                        query = SearchByElement(parameter.Key, parameter.Value, query);
                    }
                }
            }

            var list = await query.ToListAsync();
            return list;
        }

    }
}
