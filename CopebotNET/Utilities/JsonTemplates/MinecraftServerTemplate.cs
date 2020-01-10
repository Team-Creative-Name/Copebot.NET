﻿using System.Collections.Generic;
using Newtonsoft.Json;

namespace CopebotNET.Utilities.JsonTemplates {

	public class MinecraftServer {
		[JsonProperty("ip")]
		public string IpAddress { get; private set; }

		[JsonProperty("port")]
		public int Port { get; private set; }

		[JsonProperty("debug")]
		public DebugObject Debug { get; private set; }

		[JsonProperty("motd")]
		public MotdObject MessageOfTheDay { get; private set; }

		[JsonProperty("players")]
		public PlayersObject Players { get; private set; }

		[JsonProperty("version")]
		public string Version { get; private set; }

		[JsonProperty("online")]
		public bool IsOnline { get; private set; }

		[JsonProperty("protocol")]
		public int Protocol { get; private set; }

		[JsonProperty("hostname")]
		public string Hostname { get; private set; }

		[JsonProperty("icon")]
		public string ServerIcon { get; private set; }

		[JsonProperty("software")]
		public string Software { get; private set; }

		[JsonProperty("map")]
		public string WorldName { get; private set; }

		[JsonProperty("plugins")]
		public PluginsObject Plugins { get; private set; }

		[JsonProperty("mods")]
		public ModsObject Mods { get; private set; }

		[JsonProperty("info")]
		public InfoObject Info { get; private set; }
	}

	// I need so separate this a lot so I guess I'll just insert a comment here

	public struct DebugObject {
		[JsonProperty("ping")]
		public bool IsServerListPingUsed { get; private set; }

		[JsonProperty("query")]
		public bool IsQueryUsed { get; private set; }

		[JsonProperty("srv")]
		public bool IsServiceRecordUsed { get; private set; }

		[JsonProperty("querymismatch")]
		public bool DoesQueryPortMismatch { get; private set; }

		[JsonProperty("ipinsrv")]
		public bool WasIpDetectedInSrv { get; private set; }

		[JsonProperty("animatedmotd")]
		public bool HasAnimatedMotd { get; private set; }

		[JsonProperty("proxypipe")]
		public bool HasProxyPipe { get; private set; }

		[JsonProperty("cachetime")]
		public int CacheTime { get; private set; }

		[JsonProperty("api_version")]
		public string ApiVersion { get; private set; }

		[JsonProperty("dns")]
		public DnsObject Dns { get; private set; }
	}

	public struct DnsObject {
		[JsonProperty("srv")]
		public List<DnsServerObject> DnsServerList { get; private set; }

		[JsonProperty("a")]
		public List<ARecordObject> ARecordList { get; private set; }
	}

	public struct DnsServerObject {
		[JsonProperty("host")]
		public string Host { get; private set; }

		[JsonProperty("class")]
		public string Class { get; private set; }

		[JsonProperty("ttl")]
		public int Ttl { get; private set; }

		[JsonProperty("type")]
		public string Type { get; private set; }

		[JsonProperty("pri")]
		public int Pri { get; private set; }

		[JsonProperty("weight")]
		public int Weight { get; private set; }

		[JsonProperty("port")]
		public int Port { get; private set; }

		[JsonProperty("target")]
		public string Target { get; private set; }
	}

	public struct ARecordObject {
		[JsonProperty("host")]
		public string Host { get; private set; }

		[JsonProperty("class")]
		public string Class { get; private set; }

		[JsonProperty("ttl")]
		public int Ttl { get; private set; }

		[JsonProperty("type")]
		public string Type { get; private set; }

		[JsonProperty("ip")]
		public string IpAddress { get; private set; }
	}

	public struct MotdObject {
		[JsonProperty("raw")]
		public List<string> MessageRaw { get; private set; }

		[JsonProperty("clean")]
		public List<string> MessageClean { get; private set; }

		[JsonProperty("html")]
		public List<string> MessageHtml { get; private set; }
	}

	public struct PlayersObject {
		[JsonProperty("online")]
		public int OnlinePlayers { get; private set; }

		[JsonProperty("max")]
		public int MaxPlayers { get; private set; }

		[JsonProperty("list")]
		public List<string> ListPlayers { get; private set; }
	}

	public struct PluginsObject {
		[JsonProperty("names")]
		public List<string> PluginList { get; private set; }

		[JsonProperty("raw")]
		public List<string> PluginListRaw { get; private set; }
	}

	public struct ModsObject {
		[JsonProperty("names")]
		public List<string> ModList { get; private set; }

		[JsonProperty("raw")]
		public SortedList<int, string> ModListRaw { get; private set; }
	}

	public struct InfoObject {
		[JsonProperty("raw")]
		public List<string> InfoRaw { get; private set; }

		[JsonProperty("clean")]
		public List<string> InfoClean { get; private set; }

		[JsonProperty("html")]
		public List<string> InfoHtml { get; private set; }
	}
}