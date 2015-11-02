using Learning.ApiBase;
using Learning.ApiBase.Models;
using Learning.Data;
using Learning.Data.Entities;
using Learning.Web.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace Learning.Web.Controllers
{
    public class EnrollmentsController : EnrollmentsBaseController
    {
        public EnrollmentsController(ILearningRepository repo)
            : base(repo)
        {
        }
    }
}