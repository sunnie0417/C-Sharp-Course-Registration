using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AcademicManagement;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace Lab3.Pages
{
    public class RegistrationModel : PageModel
    {
        public List<Student> ?Students { get; set; }
        public List<Course> ?Courses { get; set; }
        public List<AcademicRecord> ?RegisteredCourses { get; set; }
        public SelectList ?StudentSelectList { get; set; }

        [BindProperty(SupportsGet = true)]
        public string ?SelectedStudentId { get; set; }

        [BindProperty]
        public List<string> ?SelectedCourses { get; set; }

        [BindProperty]
        public Dictionary<string, string> ?Grades { get; set; }

        [BindProperty(SupportsGet = true)]
        public string ?CourseSortOrder { get; set; }

        [BindProperty(SupportsGet = true)]
        public string ?AcademicRecordSortOrder { get; set; }

        public bool ShowCourses { get; set; }
        public bool NoCoursesRegistered { get; set; }
        public bool ShowRegistrationSuccess { get; set; }
        public bool AllowRegistration { get; set; }

        public string ?ErrorMessage { get; set; }

        public void OnGet()
        {
            LoadStudents();

            var selectedStudentIdInQuery = Request.Query.ContainsKey("SelectedStudentId");

            if (selectedStudentIdInQuery)
            {
                if (string.IsNullOrEmpty(SelectedStudentId))
                {
                    ErrorMessage = "You must select a student.";
                    ShowCourses = false;
                }
                else
                {
                    LoadStudentData();
                }
            }
            else
            {
                ShowCourses = false;
            }
        }

        public void OnPostStudentSelected()
        {
            if (string.IsNullOrEmpty(SelectedStudentId))
            {
                ErrorMessage = "You must select a student.";
                LoadStudents();
                ShowCourses = false;
                return;
            }

            if (SelectedCourses != null && SelectedCourses.Count > 0)
            {
                foreach (var courseCode in SelectedCourses)
                {
                    var record = new AcademicRecord(SelectedStudentId, courseCode);
                    try
                    {
                        DataAccess.AddAcademicRecord(record);
                    }
                    catch (Exception ex)
                    {
                        ErrorMessage = ex.Message;
                    }
                }

                HttpContext.Session.SetString("SelectedStudentId", SelectedStudentId);

                LoadStudentData();
            }
            else
            {
                ErrorMessage = "You must select at least one course";
                NoCoursesRegistered = false;
                ShowCourses = true;
                AllowRegistration = true;
                LoadStudentData();
            }
        }

        public void OnPostSubmitGrades()
        {
            if (string.IsNullOrEmpty(SelectedStudentId))
            {
                ErrorMessage = "You must select a student.";
                LoadStudents();
                ShowCourses = false;
                return;
            }

            LoadStudentData();

            if (Grades != null)
            {
                foreach (var record in RegisteredCourses!)
                {
                    if (Grades.ContainsKey(record.CourseCode))
                    {
                        var gradeValue = Grades[record.CourseCode];
                        if (double.TryParse(gradeValue, out double grade))
                        {
                            record.Grade = grade;
                        }
                        else
                        {
                            record.Grade = -100.0;
                        }
                    }
                }
            }

            HttpContext.Session.SetString("SelectedStudentId", SelectedStudentId);

            ShowRegistrationSuccess = true;
            ShowCourses = true;
            AllowRegistration = false;
        }

        private void LoadStudents()
        {
            Students = DataAccess.GetAllStudents();
            StudentSelectList = new SelectList(Students, "StudentId", "StudentName", SelectedStudentId);
        }

        private void LoadStudentData()
        {
            LoadStudents();

            Courses = DataAccess.GetAllCourses();
            RegisteredCourses = DataAccess.GetAcademicRecordsByStudentId(SelectedStudentId);

            if (RegisteredCourses == null || RegisteredCourses.Count == 0)
            {
                NoCoursesRegistered = true;
                ShowRegistrationSuccess = false;
                ShowCourses = true;
                AllowRegistration = true;
            }
            else
            {
                NoCoursesRegistered = false;
                ShowRegistrationSuccess = true;
                ShowCourses = true;
                AllowRegistration = false;
            }

            ApplySorting();
        }

        private void ApplySorting()
        {
            if (!string.IsNullOrEmpty(CourseSortOrder))
            {
                if (CourseSortOrder == "CourseCode")
                {
                    Courses = Courses!.OrderBy(c => c.CourseCode).ToList();
                }
                else if (CourseSortOrder == "CourseTitle")
                {
                    Courses = Courses!.OrderBy(c => c.CourseTitle).ToList();
                }
            }

            if (!string.IsNullOrEmpty(AcademicRecordSortOrder))
            {
                if (AcademicRecordSortOrder == "CourseCode")
                {
                    RegisteredCourses = RegisteredCourses!.OrderBy(r => r.CourseCode).ToList();
                }
                else if (AcademicRecordSortOrder == "CourseTitle")
                {
                    RegisteredCourses = RegisteredCourses!.OrderBy(r =>
                        Courses!.FirstOrDefault(c => c.CourseCode == r.CourseCode)?.CourseTitle
                    ).ToList();
                }
                else if (AcademicRecordSortOrder == "Grade")
                {
                    RegisteredCourses = RegisteredCourses!.OrderBy(r => r.Grade).ToList();
                }
            }
        }

        public string GetGrade(string courseCode)
        {
            var record = RegisteredCourses!.FirstOrDefault(r => r.CourseCode == courseCode);
            if (record != null && record.Grade != -100.0)
            {
                return record.Grade.ToString();
            }
            else
            {
                return string.Empty;
            }
        }
    }
}
