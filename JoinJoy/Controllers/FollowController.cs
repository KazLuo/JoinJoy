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
    [OpenApiTag("Follow", Description = "追蹤功能")]
    [RoutePrefix("follow")]
    public class FollowController : ApiController
    {
        //private Context db = new Context();
        //#region"追蹤會員"
        //[HttpPost]
        //[JwtAuthFilter]
        //[Route("followmember")]
        //public IHttpActionResult followmember 
        //{ 

        //}
    }
}
