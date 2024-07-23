using System;
using System.Text.Json;
using System.Threading.Tasks;
using Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Hosting;
using Orleans.Storage;

namespace Server
{

	public class Program
	{
		static IHost host = null!;
		static bool exitProgramm;

		public static Task Main(string[] args)
		{
			return StartHost();
		}

		/// <summary>
		/// Запускает хост сервера
		/// </summary>
		public static async Task StartHost()
		{
			host = new HostBuilder()
				.UseOrleans(siloBuilder =>
				{
					siloBuilder.UseLocalhostClustering();
					//Настраиваем хранилище в базе данных Npgsql
					siloBuilder.AddAdoNetGrainStorageAsDefault(options =>
					{
						options.Invariant = "Npgsql";
						options.ConnectionString = "Host=localhost;Port=5432;Database=OrleansStorage;Username=postgres;Password=root";
						options.GrainStorageSerializer = new SystemTextJsonSerializer();
					});
					siloBuilder.ConfigureLogging(logging => logging.AddConsole());
				})
				.Build();

			if (host == null) return;

			Console.CancelKeyPress += CancelKeyPress;
			await host.StartAsync();
			await GameLoop();
			await host.StopAsync();
			host.Dispose();
		}

		/// <summary>
		/// Реализует сериализацию данных с использованием System.Text.Json
		/// </summary>
		public class SystemTextJsonSerializer : IGrainStorageSerializer
		{
			public BinaryData Serialize<T>(T input)
			{
				return new BinaryData(JsonSerializer.SerializeToUtf8Bytes(input));
			}

			public T? Deserialize<T>(BinaryData input)
			{
				return input.ToObjectFromJson<T>();
			}
		}

		/// <summary>
		/// Основной игровой цикл сервера
		/// </summary>
		public static async Task GameLoop()
		{
			while (!exitProgramm)
			{
				var client = host.Services.GetRequiredService<IClusterClient>();
				var queueGrain = client.GetGrain<IQueueGrain>("playerQueue");
				var rooms = await queueGrain.GetGameRooms();
				foreach (var room in rooms)
				{
					var gameRoom = client.GetGrain<IRoomGrain>(room);
					await gameRoom.Update();
					await Task.Delay(100);
				}
				await Task.Delay(500);
			}
		}

		private static void CancelKeyPress(object? sender, ConsoleCancelEventArgs e)
		{
			Console.WriteLine("Start exit");
			e.Cancel = true;
			exitProgramm = true;
		}
	}
}
