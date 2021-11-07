using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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

        /*// GET: api/Pizzas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TModel>>> GetAll()
        {
            return await _context.Set<TModel>().Where(x => x.Active == true).ToListAsync();
        }*/

        // GET: api/Pizzas?range=0-5 | api/Pizzas?asc=price
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TModel>>> GetRanged(string range, string asc, string desc)
        {
            if(range != null)
            {

                string[] limits = range.Split('-');
                int limitMin = int.Parse(limits[0]) - 1;
                int limitMax = int.Parse(limits[1]) - int.Parse(limits[0]) + 1;

                var list = await Task.Run(() => _context.Set<TModel>().Where(x => x.Active == true).ToList().GetRange(limitMin, limitMax));

                return list;
            }
            if(asc != null)
            {

                var source = _context.Set<TModel>();

                // LAMBDA: x => x.[PropertyName]
                var parameter = Expression.Parameter(typeof(TModel), "x");

                Expression property = Expression.Property(parameter, asc);
                var lambda = Expression.Lambda(property, parameter);

                // REFLECTION: source.OrderBy(x => x.Property)
                var orderByMethod = typeof(Queryable).GetMethods().First(x => x.Name == "OrderBy" && x.GetParameters().Length == 2);
                var orderByGeneric = orderByMethod.MakeGenericMethod(typeof(TModel), property.Type);
                var result = orderByGeneric.Invoke(null, new object[] { source, lambda });

                return await ((IOrderedQueryable<TModel>)result).ToListAsync();
            }
            if (desc != null)
            {

                var source = _context.Set<TModel>();

                // LAMBDA: x => x.[PropertyName]
                var parameter = Expression.Parameter(typeof(TModel), "x");

                Expression property = Expression.Property(parameter, desc);
                var lambda = Expression.Lambda(property, parameter);

                // REFLECTION: source.OrderBy(x => x.Property)
                var orderByMethod = typeof(Queryable).GetMethods().First(x => x.Name == "OrderByDescending" && x.GetParameters().Length == 2);
                var orderByGeneric = orderByMethod.MakeGenericMethod(typeof(TModel), property.Type);
                var result = orderByGeneric.Invoke(null, new object[] { source, lambda });

                return await ((IOrderedQueryable<TModel>)result).ToListAsync();
            }
            else
            {
                return await _context.Set<TModel>().Where(x => x.Active == true).ToListAsync();
            }
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TModel>>> GetAllAsync([FromQuery] string fields)
        {
            var query = _context.Set<TModel>().AsQueryable();

            if (Request != null)
            {
                var parameters = Request.Query.Where((x) => x.Key != "fields" && x.Key != "range" && x.Key != "asc" && x.Key !="desc");

                if (parameters.Count() > 0)
                {
                    foreach (var parameter in parameters)
                    {
                        query = query.filter(parameter.Key, parameter.Value);
                    }
                }
            }

            return query.ToList();
        }

    }
}
