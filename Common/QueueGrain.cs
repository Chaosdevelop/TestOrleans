using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Orleans;

namespace Common
{
	/// <summary>
	/// Зерно очереди
	/// </summary>
	public class QueueGrain : Grain, IQueueGrain
	{
		private readonly List<string> players = new List<string>();
		private readonly List<string> rooms = new List<string>();

		public Task AddPlayer(string playerId)
		{
			players.Add(playerId);
			TryStartGame();
			return Task.CompletedTask;
		}

		/// <summary>
		/// Пытается запустить игру при наличии двух игроков в очереди
		/// </summary>
		public async Task TryStartGame()
		{
			if (players.Count < 2) return;

			var roomId = Guid.NewGuid().ToString();
			var roomGrain = GrainFactory.GetGrain<IRoomGrain>(roomId);
			rooms.Add(roomId);

			foreach (var player in players.Take(2))
			{
				await roomGrain.AddPlayer(player);
				var playerGrain = GrainFactory.GetGrain<IPlayerGrain>(player);
				await playerGrain.SetRoomId(roomId);
			}

			players.RemoveRange(0, 2);
			Console.WriteLine($"New game started {roomId}");
			await roomGrain.GameStart();
		}

		public Task<List<string>> GetGameRooms()
		{
			return Task.FromResult(rooms);
		}
	}
}
