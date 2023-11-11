using System.ComponentModel.DataAnnotations;

namespace ScoreBoardHub.Models;

public sealed class ScoreBoard
{
    [Required(ErrorMessage = "Name is required")]
    public string Name { get; set; }

    [Required(ErrorMessage = "Subtitle is required")]
    public string Subtitle { get; set; }

    [Required(ErrorMessage = "Background image is required")]
    public string BackgroundImageUrl { get; set; }
}
