using System.Threading.Tasks;
using DSharpPlus.EventArgs;

namespace CopebotNET {
	public static class EventHandlers {

		public static async Task Bot_PinsUpdated(ChannelPinsUpdateEventArgs e) {
			var test = e.Channel.GetPinnedMessagesAsync().Result.Count;

			await e.Channel.SendMessageAsync(test.ToString());

			/*e.Channel.CloneAsync();
			e.Channel.ModifyAsync(channel => channel.Name="");*/

			if (test == 50)
				await e.Channel.SendMessageAsync("Channel out of pins!");
		}

		public static async Task Bot_MessageCreated(MessageCreateEventArgs e) {
			if (e.Message.Content.ToLower().Equals("test") && !e.Message.Author.IsBot)
				await e.Message.RespondAsync("test!!");
		}
	}
}