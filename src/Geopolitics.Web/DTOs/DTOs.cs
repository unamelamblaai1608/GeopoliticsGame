using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Geopolitics.Web.DTOs;

public class RegisterRequest
{
    [JsonPropertyName("username")]
    [Required(ErrorMessage = "Username is required.")]
    [StringLength(30, MinimumLength = 3, ErrorMessage = "Username must be 3–30 characters.")]
    [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Username may only contain letters, numbers and underscores.")]
    public string Username { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email address.")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("password")]
    [Required(ErrorMessage = "Password is required.")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
    public string Password { get; set; } = string.Empty;
}

public class LoginRequest
{
    [JsonPropertyName("identifier")]
    [Required(ErrorMessage = "Username or email is required.")]
    public string Identifier { get; set; } = string.Empty;

    [JsonPropertyName("password")]
    [Required(ErrorMessage = "Password is required.")]
    public string Password { get; set; } = string.Empty;
}

public class CreateGameRequest
{
    [JsonPropertyName("selectedCountryId")]
    [StringLength(10, ErrorMessage = "Country ID too long.")]
    public string? SelectedCountryId { get; set; }

    [JsonPropertyName("title")]
    [StringLength(80, MinimumLength = 1, ErrorMessage = "Title must be 1–80 characters.")]
    public string? Title { get; set; }
}

public class GameActionRequest
{
    [JsonPropertyName("actionType")]
    [Required(ErrorMessage = "actionType is required.")]
    public string ActionType { get; set; } = string.Empty;

    [JsonPropertyName("payload")]
    public System.Text.Json.JsonElement? Payload { get; set; }
}
