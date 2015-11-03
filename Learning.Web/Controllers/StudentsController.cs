using Learning.ApiBase;
using Learning.ApiBase.Models;
using Learning.Data;
using Learning.Web.Filters;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http;

namespace Learning.Web.Controllers
{
    [RoutePrefix("api/students")]
    public class StudentsController : StudentsBaseController
    {
        public StudentsController(ILearningRepository repo)
            : base(repo)
        {
        }
        
        [VersionedRoute("~/api/students", 1, "Students")]
        public override IEnumerable<StudentBaseModel> Get(int page = 0, int pageSize = 10)
        {
            return base.Get(page, pageSize);
        }
        
        [LearningAuthorize]
        [Route("{userName}")]
        public override HttpResponseMessage Get(string userName)
        {
            return base.Get(userName);
        }
    }
}
