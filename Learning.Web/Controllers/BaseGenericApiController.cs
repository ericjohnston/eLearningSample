using Learning.Data;
using Learning.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Learning.Web.Controllers
{
    public class BaseGenericApiController<T, U> : ApiController where T : class where U : class
    {
        private ILearningRepository _repo;
        private ModelFactory _modelFactory;

        public BaseGenericApiController(ILearningRepository repo)
        {
            _repo = repo;
        }

        protected ILearningRepository TheRepository
        {
            get
            {
                return _repo;
            }
        }

        protected ModelFactory TheModelFactory
        {
            get
            {
                if (_modelFactory == null)
                {
                    _modelFactory = new ModelFactory(Request, TheRepository);
                }
                return _modelFactory;
            }
        }

        public IEnumerable<U> Get()
        {
            IQueryable<T> query;

            query = TheRepository.GetAllEntities<T>().AsQueryable<T>();

            var results = query
                .ToList()
                .Select(s => TheModelFactory.Create<T>(s));

            return results;
        }

        public HttpResponseMessage Get(int id)
        {
            try
            {
                var course = TheRepository.GetCourse(id);
                if (course != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, TheModelFactory.Create(course));
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
    }
}
