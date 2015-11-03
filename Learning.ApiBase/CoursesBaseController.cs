﻿using Learning.ApiBase.Models;
using Learning.Data;
using Learning.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Routing;

namespace Learning.ApiBase
{
    public class CoursesBaseController : BaseApiController
    {
        public CoursesBaseController(ILearningRepository repo) : base(repo)
        {
            //
        }

        [Route(Name = "Courses")]
        public virtual Object Get(int page = 0, int pageSize = 10)
        {
            IQueryable<Course> query;

            query = TheRepository.GetAllCourses().OrderBy(c => c.CourseSubject.Id);
            var totalCount = query.Count();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            var urlHelper = new UrlHelper(Request);
            var prevLink = page > 0 ? urlHelper.Link("Courses", new { page = page - 1 }) : "";
            var nextLink = page < totalPages - 1 ? urlHelper.Link("Courses", new { page = page + 1 }) : "";

            var results = query
                .Skip(pageSize * page)
                .Take(pageSize)
                .ToList()
                .Select(s => TheModelFactory.Create(s));

            return new
            {
                TotalCount = totalCount,
                TotalPages = totalPages,
                PrevPageLink = prevLink,
                NextPageLink = nextLink,
                Results = results
            };
        }

        [Route("{id:int}")]
        public IHttpActionResult Get(int id)
        {
            try
            {
                var course = TheRepository.GetCourse(id);
                if (course != null)
                {
                    return Ok<CourseModel>(TheModelFactory.Create(course));
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        public HttpResponseMessage Post([FromBody] CourseModel courseModel)
        {
            try
            {
                var entity = TheModelFactory.Parse(courseModel);

                if (entity == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Could not read subject/tutor from body");
                }

                if (TheRepository.Insert(entity) && TheRepository.SaveAll())
                {
                    return Request.CreateResponse(HttpStatusCode.Created, TheModelFactory.Create(entity));
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Could not save to the database.");
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }
        }

        [HttpPatch]
        [HttpPut]
        public HttpResponseMessage Put(int id, [FromBody] CourseModel courseModel)
        {
            try
            {
                var updatedCourse = TheModelFactory.Parse(courseModel);

                if (updatedCourse == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Could not read subject/tutor from body");
                }

                var originalCourse = TheRepository.GetCourse(id, false);

                if (originalCourse == null || originalCourse.Id != id)
                {
                    return Request.CreateResponse(HttpStatusCode.NotModified, "Course is not found");
                }
                else
                {
                    updatedCourse.Id = id;
                }

                if (TheRepository.Update(originalCourse, updatedCourse) && TheRepository.SaveAll())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, TheModelFactory.Create(updatedCourse));
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.NotModified);
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }
        }

        public HttpResponseMessage Delete(int id)
        {
            try
            {
                var course = TheRepository.GetCourse(id);

                if (course == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound);
                }

                if (course.Enrollments.Count > 0)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Cannot delete course, student(s) are enrolled in course.");
                }

                if (TheRepository.DeleteCourse(id) && TheRepository.SaveAll())
                {
                    return Request.CreateResponse(HttpStatusCode.OK);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest);
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        [Route("~/api/subject/{subjectid:int}/courses")]
        public HttpResponseMessage GetCoursesBySubject(int subjectid)
        {
            Learning.Data.LearningContext ctx = null;
            Learning.Data.ILearningRepository repo = null;
            IQueryable<Course> query;

            try
            {
                ctx = new Learning.Data.LearningContext();
                repo = new Learning.Data.LearningRepository(ctx);

                query = repo.GetCoursesBySubject(subjectid);
                var coursesList = query.Select(c => c.Name).ToList();
                if (coursesList != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, coursesList);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound);
                }

            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }
            finally
            {
                ctx.Dispose();
            }
        }
    }
}
