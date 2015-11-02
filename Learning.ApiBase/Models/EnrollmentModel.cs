using System;

namespace Learning.ApiBase.Models
{
    public class EnrollmentModel
    {
        public DateTime EnrollmentDate { get; set; }
        public CourseModel Course { get; set; }
    }
}