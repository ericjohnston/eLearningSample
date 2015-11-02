using Learning.ApiBase;
using Learning.Data;
using System.Web.Http;

namespace Learning.Web.Controllers
{
    [RoutePrefix("api/courses")]
    public class CoursesController : CoursesBaseController
    {
        public CoursesController(ILearningRepository repo) : base(repo)
        {
            //
        }
    }
}
