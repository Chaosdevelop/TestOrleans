using System;
using System.Threading.Tasks;
using Orleans;
using Orleans.Runtime;

namespace Common
{
	/// <summary>
	/// Зерно игрока
	/// </summary>
	public class PlayerGrain : Grain, IPlayerGrain
	{
		private string name = null!;
		private string? roomId;
		private int number;
		private bool waiting;
		private RoundResult lastRoundResult;

		private readonly IPersistentState<PlayerState> state;

		/// <summary>
		/// Конструктор зерна игрока куда передаем ссылку на состояние очков игрока.
		/// </summary>
		/// <param name="state">Состояние игрока</param>
		public PlayerGrain([PersistentState("playerState")] IPersistentState<PlayerState> state)
		{
			this.state = state;
			number = -1;
		}

		public Task SetName(string name)
		{
			this.name = name;
			return Task.CompletedTask;
		}

		public Task<string> GetName()
		{
			return Task.FromResult(name);
		}

		public Task<int> GetScore()
		{
			return Task.FromResult(state.State.Score);
		}

		public Task SetRoomId(string roomId)
		{
			this.roomId = roomId;
			return Task.CompletedTask;
		}

		public Task<string?> GetRoomId()
		{
			return Task.FromResult(roomId);
		}
		/// <summary>
		/// Устанавливаем предполагаемое число и проверяем на корректность.
		/// </summary>
		/// <param name="number">Предполагаемое число.</param>
		public Task<bool> SetGuess(int number)
		{
			this.number = number;
			if (number >= RoomGrain.MIN_NUMBER && number <= RoomGrain.MAX_NUMBER)
			{
				waiting = true;
				return Task.FromResult(true);
			}
			return Task.FromResult(false);
		}

		public Task<int> GetGuess()
		{
			return Task.FromResult(number);
		}

		public Task<bool> Waiting()
		{
			return Task.FromResult(waiting);
		}

		/// <summary>
		/// Завершаем раунд для игрока.
		/// </summary>
		/// <param name="result">Результат раунда</param>
		public async Task EndRound(RoundResult result)
		{
			lastRoundResult = result;
			switch (result)
			{
				case RoundResult.Win:
					state.State.Score++;
					await state.WriteStateAsync();
					Console.WriteLine($"Round ended with victory of {name}");
					break;
				case RoundResult.Lose:
					break;
				case RoundResult.Draw:
					Console.WriteLine("Round ended with draw");
					break;
				default:
					break;
			}
			number = -1;
			waiting = false;
		}

		public Task<RoundResult> GetRoundResult()
		{
			return Task.FromResult(lastRoundResult);
		}
	}
}
