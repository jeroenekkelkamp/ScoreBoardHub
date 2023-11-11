using System.ComponentModel.DataAnnotations;

namespace ScoreBoardHub.Models;

public sealed class ScoreBoardEntry
{
    public string RowKey { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "Player name is required")]
    public string PlayerName { get; set; }

    [Required(ErrorMessage = "Score is required")]
    [Range(0, int.MaxValue, ErrorMessage = "Let's stay positive")]
    public int Score { get; set; }
}