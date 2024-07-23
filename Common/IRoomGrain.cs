using System.Threading.Tasks;
using Orleans;

namespace Common
{
	/// <summary>
	/// Интерфейс зерна комнаты
	/// </summary>
	public interface IRoomGrain : IGrainWithStringKey
	{
		Task AddPlayer(string playerId);
		Task HandleGuesses(int guess1, int guess2);
		Task GameStart();
		Task Update();
	}
}
