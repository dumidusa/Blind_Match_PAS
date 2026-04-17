using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BlindMatchPAS.ViewModels;

public class ProjectViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, MinimumLength = 5, ErrorMessage = "Title must be 5–200 characters")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Abstract is required")]
    [StringLength(2000, MinimumLength = 50, ErrorMessage = "Abstract must be at least 50 characters")]
    public string Abstract { get; set; } = string.Empty;

    [Required(ErrorMessage = "Tech Stack is required")]
    [RegularExpression(@"^[a-zA-Z0-9\s,.\-/#+]+$",
        ErrorMessage = "Tech Stack can only contain letters, numbers, commas, and common symbols")]
    [StringLength(500)]
    public string TechStack { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please select a Research Area")]
    public int ResearchAreaId { get; set; }

    // Populated in the controller for the dropdown
    public List<SelectListItem> ResearchAreas { get; set; } = new();
}