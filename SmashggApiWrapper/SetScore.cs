namespace Fizzi.Libraries.SmashggApiWrapper;

public class SetScore
{
	public int Player1Score { get; set; }

	public int Player2Score { get; set; }

	public static SetScore Create(int player1Score, int player2Score)
	{
		return new SetScore
		{
			Player1Score = player1Score,
			Player2Score = player2Score
		};
	}

	public override string ToString()
	{
		return $"{Player1Score}-{Player2Score}";
	}
}
