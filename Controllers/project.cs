using System.ComponentModel.DataAnnotations;

namespace BLIND_MATCH_PAS.Models
{
    public enum ProjectStatus
    {
        Pending,
        UnderReview,
        Matched
    }

    public class StudentProject
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [Required]
        public string Abstract { get; set; }

        [Required]
        public string TechnicalStack { get; set; }

        [Required]
        public string ResearchArea { get; set; }

        public ProjectStatus Status { get; set; } = ProjectStatus.Pending;

        // Foreign key to the student (ASP.NET Identity user)
        public string? StudentId { get; set; }
        public string? SupervisorId { get; set; }
    }
}