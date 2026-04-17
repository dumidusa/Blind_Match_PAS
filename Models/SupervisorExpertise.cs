using System.ComponentModel.DataAnnotations;
 
namespace BlindMatchPAS.Models;
 
public class SupervisorExpertise
{
    public int Id { get; set; }
 
    [Required]
    public int SupervisorId { get; set; }
    public User? Supervisor { get; set; }
 
    [Required]
    public int ResearchAreaId { get; set; }
    public ResearchArea? ResearchArea { get; set; }
}
 