using Learning.ApiBase.Models;
using Learning.Data;
using Learning.Data.Entities;
using Learning.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Routing;

namespace Learning.Web.Controllers
{
    [RoutePrefix("api/v2/students")]
    public class StudentsV2Controller : StudentsController
    {
        public StudentsV2Controller(ILearningRepository repo)
            : base(repo)
        {
        }

        [VersionedRoute("~/api/students", 2, "Students2")]
        public IEnumerable<StudentV2BaseModel> GetV2(int page = 0, int pageSize = 10)
        {
            IQueryable<Student> query;

            query = TheRepository.GetAllStudentsWithEnrollments().OrderBy(c => c.LastName);

            var totalCount = query.Count();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            var urlHelper = new UrlHelper(Request);
            var prevLink = page > 0 ? urlHelper.Link("Students2", new { page = page - 1, pageSize = pageSize }) : "";
            var nextLink = page < totalPages - 1 ? urlHelper.Link("Students2", new { page = page + 1, pageSize = pageSize }) : "";

            var paginationHeader = new
            {
                TotalCount = totalCount,
                TotalPages = totalPages,
                PrevPageLink = prevLink,
                NextPageLink = nextLink
            };

            System.Web.HttpContext.Current.Response.Headers.Add("X-Pagination",
                                                                Newtonsoft.Json.JsonConvert.SerializeObject(paginationHeader));

            var results = query
                        .Skip(pageSize * page)
                        .Take(pageSize)
                        .ToList()
                        .Select(s => TheModelFactory.CreateV2Summary(s));

            return results;
        }
    }
}
