using BiitSocioApis.classes;
using BiitSocioApis.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace BiitSocioApis.Controllers
{
    public class ProductController : ApiController
    {
        [HttpPost]
        public HttpResponseMessage saveOrder(List<Product> products ) {
			try
			{

				foreach (var item in products)
				{
					common.db.Products.Add(item);
				}
				return Request.CreateResponse(HttpStatusCode.OK,"Saved");
			}
			catch (Exception ex)
			{

				return Request.CreateResponse(HttpStatusCode.InternalServerError,"Something gome wrong");
			}
        }
    }
}
