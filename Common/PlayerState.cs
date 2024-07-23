using System;

namespace Common
{
	/// <summary>
	/// Состояние игрока для хранилища.
	/// </summary>
	[Serializable]
	public class PlayerState
	{
		/// <summary>
		/// Текущее количество очков игрока.
		/// </summary>
		public int Score { get; set; }
	}
}
