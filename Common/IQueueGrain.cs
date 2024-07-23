using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;

namespace Common
{
	/// <summary>
	/// Интерфейс зерна очереди/лобби
	/// </summary>
	public interface IQueueGrain : IGrainWithStringKey
	{
		Task AddPlayer(string playerId);
		Task<List<string>> GetGameRooms();
	}
}
