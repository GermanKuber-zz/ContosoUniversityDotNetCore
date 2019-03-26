﻿namespace ContosoUniversity.IntegrationTests.Features.Departments
{
    using ContosoUniversity.Features.Departments;
    using Models;
    using Shouldly;
    using System;
    using System.Threading.Tasks;
    using Xunit;
    using static SliceFixture;

    public class DetailsTests : IntegrationTestBase
    {
        [Fact]
        public async Task Should_get_department_details()
        {
            var adminId = await SendAsync(new ContosoUniversity.Features.Instructors.CreateEdit.Command
            {
                FirstMidName = "George",
                LastName = "Costanza",
                HireDate = DateTime.Today,
            });

            var dept = new Department
            {
                Name = "History",
                InstructorID = adminId,
                Budget = 123m,
                StartDate = DateTime.Today
            };
            await InsertAsync(dept);

            var query = new Details.Query
            {
                Id = dept.Id
            };

            var result = await SendAsync(query);
            var admin = await FindAsync<Instructor>(adminId);

            result.ShouldNotBeNull();
            result.Name.ShouldBe(dept.Name);
            result.AdministratorFullName.ShouldBe(admin.FullName);
        }

    }
}