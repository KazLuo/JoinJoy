using NSwag.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using JoinJoy.Models;
using JoinJoy.Models.ViewModels;
using JoinJoy.Security;

namespace JoinJoy.Controllers
{
    [OpenApiTag("Search", Description = "搜尋功能")]
    [RoutePrefix("search")]
    public class SearchController : ApiController
    {
        private Context db = new Context();

        [HttpGet]
        [Route("search/stores")]
        public IHttpActionResult SearchStores(string city, string district = "")//.NET 8.0可以將string district ="" 改成,string? district
        {
            // 確保縣市輸入不為空
            if (string.IsNullOrEmpty(city))
            {
                return Content(HttpStatusCode.BadRequest, "縣市不可為空");
            }

            // 模糊搜尋店家
            var query = db.Stores.AsQueryable();

            query = query.Where(store => store.Address.Contains(city));

            if (!string.IsNullOrEmpty(district))
            {
                query = query.Where(store => store.Address.Contains(district));
            }

            var matchedStores = query.Select(store => new
            {
                store.Id,
                store.Name,
                store.Address,
                
            })
                .ToList();

            if (matchedStores == null || !matchedStores.Any())
            {
                return NotFound();
            }

            return Ok(matchedStores);
        }
    }
}
