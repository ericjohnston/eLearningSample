﻿using Learning.ApiBase.Models;
using Learning.Data;
using Learning.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Learning.ApiBase
{
    public class EnrollmentsBaseController : BaseApiController
    {
        public EnrollmentsBaseController(ILearningRepository repo)
            : base(repo)
        {
        }

        [Route("~/api/courses/{courseId}/students", Name = "Enrollments")]
        public IEnumerable<StudentBaseModel> Get(int courseId, int page = 0, int pageSize = 10)
        {
            IQueryable<Student> query;

            query = TheRepository.GetEnrolledStudentsInCourse(courseId).OrderBy(s => s.LastName);

            var totalCount = query.Count();

            System.Web.HttpContext.Current.Response.Headers.Add("X-InlineCount", totalCount.ToString());

            var results = query
            .Skip(pageSize * page)
            .Take(pageSize)
            .ToList()
            .Select(s => TheModelFactory.CreateSummary(s));

            return results;

        }

        //[LearningAuthorize]
        [Route("~/api/courses/{courseId}/students/{userName}")]
        public HttpResponseMessage Post(int courseId, [FromUri]string userName, [FromBody]Enrollment enrollment)
        {
            try
            {

                if (!TheRepository.CourseExists(courseId)) return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Could not find Course");

                var student = TheRepository.GetStudent(userName);
                if (student == null) return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Could not find Student");

                var result = TheRepository.EnrollStudentInCourse(student.Id, courseId, enrollment);

                if (result == 1)
                {
                    return Request.CreateResponse(HttpStatusCode.Created);
                }
                else if (result == 2)
                {
                    return Request.CreateResponse(HttpStatusCode.NotModified, "Student already enrolled in this course");
                }

                return Request.CreateResponse(HttpStatusCode.BadRequest);

            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }
        }
    }
}
