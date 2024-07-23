using System;
using System.Threading.Tasks;
using Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orleans;
using Orleans.Hosting;

namespace Client
{

	class Program
	{
		static bool exitProgramm;
		static string? playerId;

		static async Task Main(string[] args)
		{
			var host = Host.CreateDefaultBuilder()
				.UseOrleansClient(clientBuilder =>
				{
					clientBuilder.UseLocalhostClustering();
				})
				.Build();

			Console.CancelKeyPress += CancelKeyPress;
			await host.StartAsync();

			var client = host.Services.GetRequiredService<IClusterClient>();

			Console.WriteLine("Enter player name:");

			playerId = await GetPlayerInput();
			var playerGrain = client.GetGrain<IPlayerGrain>(playerId);

			if (playerId != null)
			{
				await playerGrain.SetName(playerId);
			}
			else
			{
				throw new ArgumentException("Player name can't be null.");
			}

			var queueGrain = client.GetGrain<IQueueGrain>("playerQueue");

			await queueGrain.AddPlayer(playerId);
			Console.WriteLine("Waiting for another player...");
			await GameLoop(client);
			Console.WriteLine("Exit game loop");
			await host.StopAsync();
		}

		/// <summary>
		/// Получает ввод от игрока.
		/// </summary>
		/// <returns>Строка</returns>
		static async Task<string?> GetPlayerInput()
		{
			return await ReadLineAsync();
		}

		/// <summary>
		/// Асинхронное ожидание ввода строки.
		/// </summary>
		/// <returns>Строка</returns>
		static Task<string?> ReadLineAsync()
		{
			var tcs = new TaskCompletionSource<string?>();

			Task.Run(() =>
			{
				string? input = Console.ReadLine();
				tcs.SetResult(input);
			});

			return tcs.Task;
		}

		/// <summary>
		/// Основной игровой цикл
		/// </summary>
		/// <param name="client">Клиент кластера</param>
		static async Task GameLoop(IClusterClient client)
		{
			while (!exitProgramm)
			{
				await Task.Delay(500);
				var playerGrain = client.GetGrain<IPlayerGrain>(playerId);
				var roomId = await playerGrain.GetRoomId();
				//Проверяем на наличие комнаты у игрока
				if (!string.IsNullOrEmpty(roomId))
				{
					Console.WriteLine("Enter your guess (0-100):");
					var input = await GetPlayerInput();

					if (int.TryParse(input, out int guess))
					{
						//Пытаемся задать число, если ввели некорректное значение повторяем заново
						bool correct = await playerGrain.SetGuess(guess);
						while (!correct)
						{
							Console.WriteLine("Incorrect number. Enter your guess (0-100):");
							input = await GetPlayerInput();
							int.TryParse(input, out guess);
							correct = await playerGrain.SetGuess(guess);
						}

						Console.WriteLine("Waiting for another player...");
					}
					//Ждем пока не завершится раунд
					bool wait = await playerGrain.Waiting();
					while (wait)
					{
						await Task.Delay(100);
						wait = await playerGrain.Waiting();
					}
					//Сообщаем результат раунда
					var result = await playerGrain.GetRoundResult();
					switch (result)
					{
						case RoundResult.Win:
							var score = await playerGrain.GetScore();
							Console.WriteLine($"You won. Your score: {score}");
							break;
						case RoundResult.Lose:
							Console.WriteLine("You lose");
							break;
						case RoundResult.Draw:
							Console.WriteLine("Draw");
							break;
						default:
							break;
					}
				}
			}
		}


		private static void CancelKeyPress(object? sender, ConsoleCancelEventArgs e)
		{
			e.Cancel = true;
			exitProgramm = true;
		}
	}
}
