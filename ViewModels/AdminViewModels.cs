using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BlindMatchPAS.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalProjects { get; set; }
    }

    public class EnvironmentViewModel
    {
        public string DotNetVersion { get; set; } = string.Empty;
        public string OS { get; set; } = string.Empty;
        public string EnvironmentName { get; set; } = string.Empty;
        public bool DatabaseConnected { get; set; }
    }

    public class MigrationsViewModel
    {
        public List<string> AppliedMigrations { get; set; } = new();
        public List<string> PendingMigrations { get; set; } = new();
    }

    public class RbacUserRow
    {
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }

    public class AdminCreateUserViewModel
    {
        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = string.Empty;
    }
}