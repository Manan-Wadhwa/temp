using System;
using Unity.Netcode;

namespace TcgEngine
{
	[Serializable]
	public class GameSettings : INetworkSerializable
	{
		public string server_url;

		public string game_uid;

		public string scene;

		public int nb_players;

		public GameType game_type;

		public GameMode game_mode;

		public string level;

		public static GameSettings Default => new GameSettings
		{
			server_url = "",
			game_uid = "test",
			game_type = GameType.Solo,
			game_mode = GameMode.Casual,
			nb_players = 2,
			scene = "Game",
			level = ""
		};

		public virtual bool IsHost()
		{
			if (game_type != GameType.Solo && game_type != GameType.Adventure)
			{
				return game_type == GameType.HostP2P;
			}
			return true;
		}

		public virtual bool IsOffline()
		{
			if (game_type != GameType.Solo)
			{
				return game_type == GameType.Adventure;
			}
			return true;
		}

		public virtual bool IsOnline()
		{
			if (game_type != GameType.HostP2P && game_type != GameType.Multiplayer)
			{
				return game_type == GameType.Observer;
			}
			return true;
		}

		public virtual bool IsOnlinePlayer()
		{
			if (game_type != GameType.HostP2P)
			{
				return game_type == GameType.Multiplayer;
			}
			return true;
		}

		public virtual bool IsRanked()
		{
			return game_mode == GameMode.Ranked;
		}

		public virtual string GetUrl()
		{
			if (!string.IsNullOrEmpty(server_url))
			{
				return server_url;
			}
			return NetworkData.Get().url;
		}

		public virtual string GetScene()
		{
			if (!string.IsNullOrEmpty(scene))
			{
				return scene;
			}
			return GameplayData.Get().GetRandomArena();
		}

		public virtual string GetGameModeId()
		{
			if (game_mode == GameMode.Ranked)
			{
				return "ranked";
			}
			if (game_mode == GameMode.Casual)
			{
				return "casual";
			}
			return "";
		}

		public virtual LevelData GetLevel()
		{
			if (game_type == GameType.Adventure)
			{
				return LevelData.Get(level);
			}
			return null;
		}

		public virtual void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
		{
			serializer.SerializeValue(ref server_url);
			serializer.SerializeValue(ref game_uid);
			serializer.SerializeValue(ref scene);
			serializer.SerializeValue(ref game_type, default(FastBufferWriter.ForEnums));
			serializer.SerializeValue(ref game_mode, default(FastBufferWriter.ForEnums));
			serializer.SerializeValue(ref nb_players, default(FastBufferWriter.ForPrimitives));
			serializer.SerializeValue(ref level);
		}

		public static string GetRankModeString(GameMode rank_mode)
		{
			return rank_mode switch
			{
				GameMode.Ranked => "ranked", 
				GameMode.Casual => "casual", 
				_ => "", 
			};
		}

		public static GameMode GetRankMode(string rank_id)
		{
			if (rank_id == "ranked")
			{
				return GameMode.Ranked;
			}
			_ = rank_id == "casual";
			return GameMode.Casual;
		}
	}
}
