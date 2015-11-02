using Learning.ApiBase.Models;
using Learning.Data;
using Learning.Data.Entities;
using System;
using System.Linq;
using System.Net.Http;

namespace Learning.Web.Models
{
    public static class ModelExtensions
    {
        public static StudentV2BaseModel CreateV2Summary(this ModelFactory modelFactory, Student student)
        {
            return new StudentV2BaseModel()
            {
                Url = modelFactory.ModelFactoryUrlHelper.Link("Students2", new { userName = student.UserName }),
                Id = student.Id,
                FullName = string.Format("{0} {1}", student.FirstName, student.LastName),
                Gender = student.Gender,
                EnrollmentsCount = student.Enrollments.Count(),
                CourseDuration = Math.Round(student.Enrollments.Sum(c => c.Course.Duration))
            };
        }
    }
}