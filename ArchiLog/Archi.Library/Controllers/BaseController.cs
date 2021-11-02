using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        // GET: api/Pizzas?range=0-5
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TModel>>> GetRanged(string range)
        {
            if(range == null)
            {
                return await _context.Set<TModel>().Where(x => x.Active == true).ToListAsync();
            }
            else
            {
                string[] limits = range.Split('-');
                int limitMin = int.Parse(limits[0]) - 1;
                int limitMax = int.Parse(limits[1]) - int.Parse(limits[0]) + 1;

                var list = await Task.Run(() => _context.Set<TModel>().Where(x => x.Active == true).ToList().GetRange(limitMin, limitMax));

                return list;
            }
        }
    }
}
