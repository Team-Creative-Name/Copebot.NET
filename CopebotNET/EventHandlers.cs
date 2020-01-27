using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace CopebotNET {
	public class EventHandlers
	{
		private readonly ulong _typingChannelId;
		public EventHandlers(ulong typingChannelId) {
			_typingChannelId = typingChannelId;
		}

		public async Task Bot_PinsUpdated(ChannelPinsUpdateEventArgs e) {
			var test = e.Channel.GetPinnedMessagesAsync().Result.Count;

			await e.Channel.SendMessageAsync(test.ToString());

			/*e.Channel.CloneAsync();
			e.Channel.ModifyAsync(channel => channel.Name="");*/

			if (test == 50)
				await e.Channel.SendMessageAsync("Channel out of pins!");
		}

		public async Task Bot_MessageCreated(MessageCreateEventArgs e) {
			if (e.Message.Content.ToLower().Equals("test") && !e.Message.Author.IsBot)
				await e.Message.RespondAsync("test!!");
		}

		public async Task Bot_TypingStarted(TypingStartEventArgs eventArgs) {
			var builder = new DiscordEmbedBuilder()
				.WithTitle("Typing was started!")
				.WithAuthor(eventArgs.User.Username,
					eventArgs.User.AvatarUrl,
					eventArgs.User.AvatarUrl)
				.WithTimestamp(eventArgs.StartedAt)
				.WithDescription("In server \"" +
				                 eventArgs.Guild.Name +
				                 "\",\nin channel #" +
				                 eventArgs.Channel.Name +
				                 " (<#" +
				                 eventArgs.Channel.Id +
				                 ">)");
			
			await eventArgs.Client.GetChannelAsync(_typingChannelId).Result
				.SendMessageAsync(embed: builder.Build());
		}
	}
}