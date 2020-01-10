﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using CopebotNET.Utilities.JsonTemplates;
using DSharpPlus.Entities;
using Newtonsoft.Json;

namespace CopebotNET.Utilities {
	public class MinecraftServerHelper {
		private readonly HttpClient _client;
		private readonly string _apiKey;
		private ConcurrentDictionary<string, string> _minecraftServerIconList;
		private ConcurrentDictionary<ulong, string> _defaultServerList;

		private struct ServerHelperTemplate {
			[JsonProperty("DefaultServerList")] internal ConcurrentDictionary<ulong, string> DefaultServerList;
			[JsonProperty("ServerIconList")] internal ConcurrentDictionary<string, string> ServerIconList;
		}

		public MinecraftServerHelper(HttpClient client, string apiKey) {
			_client = client;
			_apiKey = apiKey;
			if (File.Exists(Environment.CurrentDirectory + "\\minecraftServerHelper.json"))
				using (var stream =
					File.OpenRead(Path.Combine(Environment.CurrentDirectory, "minecraftServerHelper.json")))
				using (var reader = new StreamReader(stream)) {
					var json = reader.ReadToEnd();
					var serverHelper = JsonConvert.DeserializeObject<ServerHelperTemplate>(json);
					_minecraftServerIconList = serverHelper.ServerIconList;
					_defaultServerList = serverHelper.DefaultServerList;
				}

			if (_minecraftServerIconList == null)
				_minecraftServerIconList = new ConcurrentDictionary<string, string>();
			if (_defaultServerList == null)
				_defaultServerList = new ConcurrentDictionary<ulong, string>();
		}

		~MinecraftServerHelper() {
			if (_defaultServerList == null && _minecraftServerIconList == null)
				return;
			// ReSharper disable once PossibleNullReferenceException
			if (_defaultServerList.IsEmpty && _minecraftServerIconList.IsEmpty)
				return;
			var template = new ServerHelperTemplate {
				ServerIconList = _minecraftServerIconList,
				DefaultServerList = _defaultServerList
			};
			using (var file = File.CreateText(Environment.CurrentDirectory + "\\minecraftServerHelper.Json")) {
				var serializer = new JsonSerializer {
					Formatting = Formatting.Indented
				};
				serializer.Serialize(file, template);
			}
		}
		
		public static DiscordEmbed BuildSmallDiscordEmbed(ServerStatus serverStatus) {
			var builder = new DiscordEmbedBuilder {
				Title = serverStatus.Hostname,
				Color = serverStatus.IsOnline ? DiscordColor.Green : DiscordColor.Red,
				Description = serverStatus.HasInformation
					? "Message of the Day:\n```" + serverStatus.MessageOfTheDay + "```Information:\n```" +
					  serverStatus.Information + "```"
					: "```" + serverStatus.MessageOfTheDay + "```",
				ThumbnailUrl = serverStatus.IconUrl
			};
			builder.AddField("Players", serverStatus.OnlinePlayers + "/" + serverStatus.MaxPlayers, true)
				.AddField("IP Address", serverStatus.IpAddress, true)
				.AddField("Is Modded?",
					serverStatus.HasMods
						? serverStatus.HasMods + ",\n" + serverStatus.NumberOfMods + " mods"
						: serverStatus.HasMods.ToString(),
					true);
			return builder.Build();
		}

		public static DiscordEmbed BuildLargeDiscordEmbed(ServerStatus serverStatus) {

			return null;
		}

		public void AddOrUpdateDefaultServer(ulong serverId, string host) {
			_defaultServerList.AddOrUpdate(serverId, host, (id, h) => host);
		}

		public ServerStatus CheckServerStatus(string host)
			=> _checkServerStatus(host);

		public ServerStatus CheckServerStatus(ulong serverId) {
			if (!_defaultServerList.TryGetValue(serverId, out var host))
				throw new ServerNotFoundException();
			return _checkServerStatus(host);
		}

		private ServerStatus _checkServerStatus(string host) {
			MinecraftServer serverStatusFull;
			try {
				using (var json = _client.GetStringAsync("https://api.mcsrvstat.us/2/" + host))
					serverStatusFull = JsonConvert.DeserializeObject<MinecraftServer>(json.Result);
			}
			catch (HttpRequestException) {
				return new ServerStatusBuilder().Build();
			}

			if (serverStatusFull.IpAddress.Equals(""))
				return new ServerStatusBuilder().IsSuccessful().Build();

			if (!_getIconUrl(serverStatusFull.ServerIcon, out var iconUrl))
				return new ServerStatusBuilder().Build();

			var serverStatus = new ServerStatusBuilder()
				.IsSuccessful()
				.IsValidWebsite()
				.IsOnline(serverStatusFull.IsOnline)
				.SetOnlinePlayers(serverStatusFull.Players.OnlinePlayers)
				.SetMaxPlayers(serverStatusFull.Players.MaxPlayers)
				.SetIpAddress(serverStatusFull.IpAddress)
				.SetHostname(host)
				.SetIcon(iconUrl)
				.SetMessageOfTheDay(serverStatusFull.MessageOfTheDay.MessageClean);

			if (serverStatusFull.Players.ListPlayers != null)
				serverStatus.HasPlayersList()
					.SetPlayersList(serverStatusFull.Players.ListPlayers);

			if (serverStatusFull.Info.InfoClean != null)
				serverStatus.HasInformation()
					.SetInformation(serverStatusFull.Info.InfoClean);

			if (serverStatusFull.Mods.ModList != null)
				serverStatus.HasMods()
					.SetNumberOfMods(serverStatusFull.Mods.ModList.Count)
					.SetModsList(serverStatusFull.Mods.ModList);

			return serverStatus.Build();
		}

		private bool _getIconUrl(string iconDataUrl, out string iconUrl) {
			iconUrl = null;
			if (iconDataUrl == null) {
				iconUrl =
					"https://cdn.discordapp.com/attachments/658571728892592138/659891409259855902/unknown_server.png";
				return true;
			}
			if (!_client.GetAsync("https://imgbb.com/").Result.IsSuccessStatusCode)
				return false;
			if (_minecraftServerIconList.TryGetValue(iconDataUrl, out iconUrl))
				return true;
			return _uploadIcon(iconDataUrl, out iconUrl);
		}

		private bool _uploadIcon(string iconDataUrl, out string iconUrl) {
			ImageUploadResponse imageUploadResponse;
			var iconBase64 = iconDataUrl.Replace("data:image/png;base64,", "");

			using (var request = new HttpRequestMessage(new HttpMethod("POST"), "https://api.imgbb.com/1/upload")) {
				var imageRequestContent = new MultipartFormDataContent {
					{new StringContent(_apiKey), "key"},
					{new StringContent(iconBase64), "image"}
				};
				request.Content = imageRequestContent;
				try {
					var response = _client.SendAsync(request);
					imageUploadResponse =
						JsonConvert.DeserializeObject<ImageUploadResponse>(response.Result.Content.ReadAsStringAsync()
							.Result);
				}
				catch (HttpRequestException) {
					iconUrl = null;
					return false;
				}
			}

			if (!imageUploadResponse.WasSuccessful) {
				iconUrl = null;
				return false;
			}
			
			iconUrl = imageUploadResponse.ImageData.Url;
			_minecraftServerIconList.TryAdd(iconDataUrl, iconUrl);
			
			return imageUploadResponse.WasSuccessful;
		}
	}

	public class ServerNotFoundException : Exception
	{
		public ServerNotFoundException() { }
		public ServerNotFoundException(string message) : base(message) { }
		public ServerNotFoundException(string message, Exception inner) : base(message,inner) { }
	}

	internal class ServerStatusBuilder {
		private bool _wasSuccessful;
		private bool _isWebsite;
		private bool _isOnline;
		private bool _hasPlayersList;
		private bool _hasInfo;
		private bool _hasMods;
		private int _onlinePlayers;
		private int _maxPlayers;
		private int _numberOfMods;
		private string _hostname;
		private string _ipAddress;
		private string _message;
		private string _playersList;
		private string _information;
		private string _modsList;
		private string _iconUrl;

		public ServerStatusBuilder() {
			_wasSuccessful = false;
			_isWebsite = false;
			_isOnline = false;
			_hasPlayersList = false;
			_hasInfo = false;
			_hasMods = false;
			_numberOfMods = 0;
			_playersList = null;
			_modsList = null;
			_information = null;
		}

		public ServerStatusBuilder IsSuccessful() {
			_wasSuccessful = true;
			return this;
		}

		public ServerStatusBuilder IsValidWebsite() {
			_isWebsite = true;
			return this;
		}

		public ServerStatusBuilder IsOnline(bool isOnline) {
			_isOnline = isOnline;
			return this;
		}

		public ServerStatusBuilder HasPlayersList() {
			_hasPlayersList = true;
			return this;
		}

		public ServerStatusBuilder HasInformation() {
			_hasInfo = true;
			return this;
		}

		public ServerStatusBuilder HasMods() {
			_hasMods = true;
			return this;
		}

		public ServerStatusBuilder SetOnlinePlayers(int onlinePlayers) {
			_onlinePlayers = onlinePlayers;
			return this;
		}

		public ServerStatusBuilder SetMaxPlayers(int maxPlayers) {
			_maxPlayers = maxPlayers;
			return this;
		}

		public ServerStatusBuilder SetNumberOfMods(int numberOfMods) {
			_numberOfMods = numberOfMods;
			return this;
		}

		public ServerStatusBuilder SetHostname(string host) {
			_hostname = host;
			return this;
		}

		public ServerStatusBuilder SetIpAddress(string ipAddress) {
			_ipAddress = ipAddress;
			return this;
		}

		public ServerStatusBuilder SetMessageOfTheDay(IEnumerable<string> messageList) {
			foreach (var part in messageList)
				_message += part.Trim() + "\n";
			_message = _message.TrimEnd();
			return this;
		}

		public ServerStatusBuilder SetPlayersList(List<string> playersList) {
			var count = playersList.Count;
			if (count > 4) {
				var random = new Random();
				var nums = new[] {-1, -1, -1 ,-1};
				for (var i = 0; i < 4; i++) {
					int add;
					do add = random.Next(count);
					while (add == nums[0] || add == nums[1] || add == nums[2] || add == nums[3]);

					nums[i] = add;
					_playersList += playersList[add] + "\n";
				}

				_playersList = _playersList.TrimEnd();
			}
			else {
				foreach (var player in playersList)
					_playersList += player + "\n";

				_playersList = _playersList.TrimEnd();
			}
			return this;
		}

		public ServerStatusBuilder SetInformation(IEnumerable<string> infoList) {
			foreach (var part in infoList)
				_information += part.Trim() + "\n";
			_information = _information.TrimEnd();
			return this;
		}

		public ServerStatusBuilder SetModsList(List<string> modsList) {
			if (_numberOfMods > 4) {
				var random = new Random();
				var nums = new[] {-1, -1, -1 ,-1};
				for (var i = 0; i < 4; i++) {
					int add;
					do add = random.Next(_numberOfMods);
					while (add == nums[0] || add == nums[1] || add == nums[2] || add == nums[3]);

					nums[i] = add;
					_modsList += modsList[add] + "\n";
				}

				_modsList = _modsList.TrimEnd();
			}
			else {
				foreach (var mod in modsList)
					_modsList = mod + "\n";

				_modsList = _modsList.TrimEnd();
			}
			return this;
		}

		public ServerStatusBuilder SetIcon(string iconUrl) {
			_iconUrl = iconUrl;
			return this;
		}

		public ServerStatus Build() {
			var statusResult = new ServerStatus {
				WasSuccessful = _wasSuccessful,
				IsValidWebsite = _isWebsite,
				IsOnline = _isOnline,
				HasPlayersList = _hasPlayersList,
				HasInformation = _hasInfo,
				HasMods = _hasMods,
				OnlinePlayers = _onlinePlayers,
				MaxPlayers = _maxPlayers,
				NumberOfMods = _numberOfMods,
				Hostname = _hostname,
				IpAddress = _ipAddress,
				MessageOfTheDay = _message,
				PlayersList = _playersList,
				Information = _information,
				ModsList = _modsList,
				IconUrl = _iconUrl
			};

			return statusResult;
		}
	}

	public sealed class ServerStatus
	{
		public bool WasSuccessful { get; internal set; }
		public bool IsValidWebsite { get; internal set; }
		public bool IsOnline { get; internal set; }
		public bool HasPlayersList { get; internal set; }
		public bool HasInformation { get; internal set; }
		public bool HasMods { get; internal set; }
		public int OnlinePlayers { get; internal set; }
		public int MaxPlayers { get; internal set; }
		public int NumberOfMods { get; internal set; }
		public string Hostname { get; internal set; }
		public string IpAddress { get; internal set; }
		public string MessageOfTheDay { get; internal set; }
		public string PlayersList { get; internal set; }
		public string Information { get; internal set; }
		public string ModsList { get; internal set; }
		public string IconUrl { get; internal set; }

		public override string ToString() {
			return base.ToString() + ": { bool: " + WasSuccessful + ", bool: " + IsValidWebsite + ", bool: " +
			       IsOnline + ", string: \"" + Hostname + "\", string: \"" + MessageOfTheDay + "\", string: \"" +
			       IconUrl + "\" }";
		}
	}
}