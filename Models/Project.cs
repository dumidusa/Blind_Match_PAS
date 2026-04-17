using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlindMatchPAS.Models;

public class Project
{
    public int Id { get; set; }

    [Required, StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(2000)]
    public string Abstract { get; set; } = string.Empty;

    [Required, StringLength(500)]
    public string TechStack { get; set; } = string.Empty;

    // Foreign key to ResearchArea
    [Required]
    public int ResearchAreaId { get; set; }
    public ResearchArea? ResearchArea { get; set; }

    // Foreign key to the Student who submitted
    [Required]
    public int StudentId { get; set; }
    public User? Student { get; set; }

    // Status: Pending | UnderReview | Matched | Withdrawn
    [Required]
    public string Status { get; set; } = "Pending";

    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

    // Set only after a supervisor confirms the match
    public int? MatchedSupervisorId { get; set; }
    public User? MatchedSupervisor { get; set; }
}