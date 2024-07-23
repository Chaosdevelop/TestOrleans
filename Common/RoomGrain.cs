using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;

namespace Common
{
	/// <summary>
	/// Зерно комнаты
	/// </summary>
	public class RoomGrain : Grain, IRoomGrain
	{
		private readonly List<string> players = new List<string>();
		private int serverNumber;
		public const int MIN_NUMBER = 0;
		public const int MAX_NUMBER = 100;

		public Task AddPlayer(string playerId)
		{
			players.Add(playerId);
			return Task.CompletedTask;
		}

		/// <summary>
		/// Обрабатывает числа-догадки от игроков и определяет результат раунда
		/// </summary>
		/// <param name="guess1">Число, заданное первым игроком</param>
		/// <param name="guess2">Число, заданное вторым игроком</param>
		public async Task HandleGuesses(int guess1, int guess2)
		{
			var player1Grain = GrainFactory.GetGrain<IPlayerGrain>(players[0]);
			var player2Grain = GrainFactory.GetGrain<IPlayerGrain>(players[1]);

			int player1diff = Math.Abs(guess1 - serverNumber);
			int player2diff = Math.Abs(guess2 - serverNumber);

			if (player1diff < player2diff)
			{
				await player1Grain.EndRound(RoundResult.Win);
				await player2Grain.EndRound(RoundResult.Lose);
			}
			else if (player1diff > player2diff)
			{
				await player1Grain.EndRound(RoundResult.Lose);
				await player2Grain.EndRound(RoundResult.Win);
			}
			else
			{
				await player1Grain.EndRound(RoundResult.Draw);
				await player2Grain.EndRound(RoundResult.Draw);
			}
			await GameStart();
		}

		/// <summary>
		/// Запускает новый раунд игры, загадывая новое число сервера
		/// </summary>
		public Task GameStart()
		{
			serverNumber = new Random().Next(MIN_NUMBER, MAX_NUMBER + 1);
			Console.WriteLine($"Game start with number {serverNumber}");
			return Task.CompletedTask;
		}

		/// <summary>
		/// Обновляет состояние игры, проверяя готовность игроков к новому раунду
		/// </summary>
		public async Task Update()
		{
			int guess1 = await GrainFactory.GetGrain<IPlayerGrain>(players[0]).GetGuess();
			int guess2 = await GrainFactory.GetGrain<IPlayerGrain>(players[1]).GetGuess();

			if (guess1 != -1 && guess2 != -1)
			{
				await HandleGuesses(guess1, guess2);
			}
		}
	}
}
