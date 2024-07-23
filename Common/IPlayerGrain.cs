using System.Threading.Tasks;
using Orleans;

namespace Common
{
	/// <summary>
	/// Интерфейс зерна игрока
	/// </summary>
	public interface IPlayerGrain : IGrainWithStringKey
	{
		Task SetName(string name);
		Task<string> GetName();
		Task<int> GetScore();
		Task SetRoomId(string roomId);
		Task<string?> GetRoomId();
		Task<bool> SetGuess(int number);
		Task<int> GetGuess();
		Task EndRound(RoundResult result);
		Task<RoundResult> GetRoundResult();
		Task<bool> Waiting();
	}
}
