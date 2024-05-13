using System.ComponentModel.DataAnnotations.Schema;
using MPM_Betting.DataModel.Betting;

namespace MPM_Betting.DataModel.Football;

/// <summary>
/// ResultBet class that inherits from the Bet class in the Betting namespace
/// This class is used to represent a bet on the result of a football match
/// </summary>
/// <example>
/// User A bets on a football match between Team A and Team B. User A bets that Team A will win
/// The match ends with Team A winning. User A wins the bet and gets points. Otherwise he will lose his points
/// </example>
public class ResultBet : Bet
{
    /// <summary>
    /// GuessedResult property for the guessed result of a football match, uses Union
    /// </summary>
    [NotMapped]
    public EResult GuessedResult
    {
        get
        {
            return (EResult)n1;
        }
        set
        {
            n1 = (int)value;
        }
    }

    /// <summary>
    /// ActualResult property for the actual result of a football match, uses Union
    /// </summary>
    [NotMapped]
    public EResult ActualResult { 
        get
        {
            return (EResult)n2;
        }
        set
        {
            n2 = (int)value;
        } }
}

/// <summary>
/// EResult enum for the possible results in a football match
/// </summary>
public enum EResult
{
    /// <summary>
    /// Home team wins
    /// </summary>
    HomeWin,

    /// <summary>
    /// The match ends in a draw
    /// </summary>
    Draw,

    /// <summary>
    /// Away team wins
    /// </summary>
    AwayWin
}