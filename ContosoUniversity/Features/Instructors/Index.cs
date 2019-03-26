using AutoMapper.QueryableExtensions;
using ContosoUniversity.Data;
using ContosoUniversity.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ContosoUniversity.Features.Instructors
{
    //TODO : 02 - Complex Query

    public class Index
    {
        public class Query : IRequest<Model>
        {
            public int? Id { get; set; }
            public int? CourseId { get; set; }
        }

        public class Model
        {
            public int? InstructorId { get; set; }
            public int? CourseId { get; set; }

            public IList<Instructor> Instructors { get; set; }
            public IList<Course> Courses { get; set; }
            public IList<Enrollment> Enrollments { get; set; }

            public class Instructor
            {
                public int Id { get; set; }

                [Display(Name = "Last Name")]
                public string LastName { get; set; }

                [Display(Name = "First Name")]
                public string FirstMidName { get; set; }

                [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
                [Display(Name = "Hire Date")]
                public DateTime HireDate { get; set; }

                public string OfficeAssignmentLocation { get; set; }

                public IEnumerable<CourseAssignment> CourseAssignments { get; set; }
            }

            public class CourseAssignment
            {
                public int CourseId { get; set; }
                public string CourseTitle { get; set; }
            }

            public class Course
            {
                public int Id { get; set; }
                public string Title { get; set; }
                public string DepartmentName { get; set; }
            }

            public class Enrollment
            {
                [DisplayFormat(NullDisplayText = "No grade")]
                public Grade? Grade { get; set; }
                public string StudentFullName { get; set; }
            }
        }

        public class Handler : AsyncRequestHandler<Query, Model>
        {
            private readonly SchoolContext _db;

            public Handler(SchoolContext db) => _db = db;

            protected override async Task<Model> HandleCore(Query message)
            {
                var instructors = await _db.Instructors
                    .Include(i => i.CourseAssignments)
                    .ThenInclude(c => c.Course)
                    .OrderBy(i => i.LastName)
                    .ProjectTo<Model.Instructor>()
                    .ToListAsync();

                var courses = new List<Model.Course>();
                var enrollments = new List<Model.Enrollment>();

                if (message.Id != null)
                {
                    courses = await _db.CourseAssignments
                        .Where(ci => ci.InstructorID == message.Id)
                        .Select(ci => ci.Course)
                        .ProjectTo<Model.Course>()
                        .ToListAsync();
                }

                if (message.CourseId != null)
                {
                    enrollments = await _db.Enrollments
                        .Where(x => x.CourseID == message.CourseId)
                        .ProjectTo<Model.Enrollment>()
                        .ToListAsync();
                }

                var viewModel = new Model
                {
                    Instructors = instructors,
                    Courses = courses,
                    Enrollments = enrollments,
                    InstructorId = message.Id,
                    CourseId = message.CourseId
                };

                return viewModel;
            }
        }

    }
}