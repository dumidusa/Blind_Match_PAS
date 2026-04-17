using System.ComponentModel.DataAnnotations;
 
namespace BlindMatchPAS.Models;
 
public class SupervisorInterest
{
    public int Id { get; set; }
 
    [Required]
    public int SupervisorId { get; set; }
    public User? Supervisor { get; set; }
 
    [Required]
    public int ProjectId { get; set; }
    public Project? Project { get; set; }
 
    // "Interested" → supervisor expressed interest
    // "Confirmed"  → supervisor confirmed the match (triggers identity reveal)
    [Required]
    public string Status { get; set; } = "Interested";
 
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ConfirmedAt { get; set; }
}
 