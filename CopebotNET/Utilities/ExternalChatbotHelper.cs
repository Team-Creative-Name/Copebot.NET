using System.Threading.Tasks;
using DSharpPlus;

namespace CopebotNET.Utilities
{
	public class ExternalChatbotHelper
	{
		public DiscordClient HelperClient { get; private set; }

		public ExternalChatbotHelper(string helperToken) {
			RunHelperBot(helperToken).GetAwaiter().GetResult();
		}

		private async Task RunHelperBot(string helperToken) {
			var config = new DiscordConfiguration
			{
				Token = helperToken,
				TokenType = TokenType.Bot,
				MessageCacheSize = 0
			};
			
			HelperClient = new DiscordClient(config);

			await HelperClient.ConnectAsync();
			
			await Task.Delay(-1);
		}
	}
}