using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CopebotNET.Utilities;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity;
using DSharpPlus.VoiceNext;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace CopebotNET
{
	internal class Bot
	{
		private static readonly HttpClient HttpClient = new HttpClient();
		private DiscordClient Client { get; set; }
		private static VoiceNextExtension _voice;
		private CommandsNextExtension Commands { get; set; }

		public static void Main() {
			var bot = new Bot();
			
			bot.RunCopebot().GetAwaiter().GetResult();
		}

		private async Task RunCopebot() {
            string json;
            using (var stream = File.OpenRead(Path.Combine(Environment.CurrentDirectory, "config.json")))
            using (var reader = new StreamReader(stream, new UTF8Encoding(false)))
                json = await reader.ReadToEndAsync();
            
            var configJson = JsonConvert.DeserializeObject<ConfigJson>(json);
            
            var config = new DiscordConfiguration
            {
                Token = configJson.MainToken,
                TokenType = TokenType.Bot,

                AutoReconnect = true,
                LogLevel = LogLevel.Info,
                UseInternalLogHandler = true
            };
            
            var eventHandlers = new EventHandlers(configJson.TypingChannelId);
            Client = new DiscordClient(config);

            Client.MessageCreated += eventHandlers.Bot_MessageCreated;
            Client.ChannelPinsUpdated += eventHandlers.Bot_PinsUpdated;
            Client.TypingStarted += eventHandlers.Bot_TypingStarted;
            
            

            var interactivityConfig = new InteractivityConfiguration {
                Timeout = TimeSpan.FromMinutes(1.0)
            };

            Client.UseInteractivity(interactivityConfig);

            var prefixes = new List<string>{configJson.CommandPrefix};
            var minecraftServerHelper = new MinecraftServerHelper(HttpClient,configJson.ImgbbApiKey);
            var services = new ServiceCollection()
                .AddSingleton(minecraftServerHelper)
                .BuildServiceProvider();

            var commandsConfig = new CommandsNextConfiguration {
                StringPrefixes = prefixes,
                Services = services,
                EnableMentionPrefix = true,
                EnableDms = true
            };

            Commands = Client.UseCommandsNext(commandsConfig);

            Commands.RegisterCommands<UngroupedBotCommands>();
            Commands.RegisterCommands<BotUserCommands>();

            await Client.ConnectAsync();

            await Task.Delay(-1);
		}
	}
	
	internal class ConfigJson {
		[JsonProperty("main_token")]
		public string MainToken { get; private set; }
		
		[JsonProperty("helper_token")]
		public string HelperToken { get; private set; }

		[JsonProperty("prefix")]
		public string CommandPrefix { get; private set; }
        
		[JsonProperty("imgbb_api_key")]
		public string ImgbbApiKey { get; private set; }
		
		[JsonProperty("typing_channel_id")]
		public ulong TypingChannelId { get; private set; }
	}
}