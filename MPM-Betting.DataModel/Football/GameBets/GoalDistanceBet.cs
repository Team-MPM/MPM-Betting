using System.ComponentModel.DataAnnotations.Schema;
using MPM_Betting.DataModel.Betting;

namespace MPM_Betting.DataModel.Football;

/// <summary>
/// GoalDistanceBet class that inherits from the Bet class in the Betting namespace
/// Bet for guessing the distance of a scored goal
/// </summary>
public class GoalDistanceBet : Bet
{
    /// <summary>
    /// Property for the guessed goal distance, uses Union
    /// </summary>
    [NotMapped]
    public int GuessedDistance => n1;

    /// <summary>
    /// Property for the real goal distance, uses Union
    /// </summary>
    [NotMapped]
    public int RealDistance => n2;
}