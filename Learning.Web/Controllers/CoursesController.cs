using Learning.ApiBase;
using Learning.Data;
using System;
using System.Web.Http;

namespace Learning.Web.Controllers
{
    public class CoursesController : CoursesBaseController
    {
        public CoursesController(ILearningRepository repo) : base(repo)
        {
            //
        }
    }
}
