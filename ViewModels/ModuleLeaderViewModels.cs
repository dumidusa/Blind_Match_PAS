using System.ComponentModel.DataAnnotations;

namespace BlindMatchPAS.ViewModels;

public class ResearchAreaViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Name is required.")]
    [RegularExpression(@"^[a-zA-Z0-9\s\-/()+]+$",
        ErrorMessage = "Only letters, numbers, spaces, hyphens, slashes, or parentheses allowed.")]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;
}

public class CreateUserViewModel
{
    [Required(ErrorMessage = "Full name is required.")]
    [StringLength(100, MinimumLength = 2)]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Role is required.")]
    public string Role { get; set; } = string.Empty;
}

public class ReassignProjectViewModel
{
    [Required]
    public int ProjectId { get; set; }

    [Required]
    public int SupervisorId { get; set; }
}
