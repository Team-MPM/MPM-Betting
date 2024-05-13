using System.ComponentModel.DataAnnotations.Schema;

namespace MPM_Betting.DataModel.Football;

// ScoreBet class that inherits from the Bet class in the Betting namespace
// This class is used to represent a bet on the exact score of a football match
// Example: User A bets on a football match between Team A and Team B. User A bets that the match will end 2:1
// The Match ends 2:1. User A wins the bet and gets points. Otherwise he will lose his points
public class ScoreBet :Betting.Bet
{
    // Property for the guessed home score in a football match
    [NotMapped]
    public int GuessedHomeScore => n1;

    // Property for the guessed away score in a football match
    [NotMapped]
    public int GuessedAwayScore => n2;
}