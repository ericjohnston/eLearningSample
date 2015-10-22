using System;

namespace Learning.Web.Models
{
    public class EnrollmentModel
    {
        public DateTime EnrollmentDate { get; set; }
        public CourseModel Course { get; set; }
    }
}