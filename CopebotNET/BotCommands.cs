﻿using System;
 using System.Threading.Tasks;
using CopebotNET.Utilities;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;

namespace CopebotNET {
	public class UngroupedBotCommands : BaseCommandModule
	{
		public MinecraftServerHelper MinecraftServerHelper { get; }

		public UngroupedBotCommands(MinecraftServerHelper minecraftServerHelper) {
			MinecraftServerHelper = minecraftServerHelper;
		}
		
		[Command("test")]
		[Description("A test command!")]
		public async Task Test(CommandContext context) {
			await context.TriggerTypingAsync();

			await context.RespondAsync("argument: " + context.RawArgumentString);
		}

		[Command("test2")]
		[Description("Another test command, this time for interactivity!")]
		public async Task Test2(CommandContext context) {
			var interactivity = context.Client.GetInteractivity();

			await context.RespondAsync("Post a message within one minute!");

			var test = await interactivity.WaitForMessageAsync(intContext => intContext.Author.Id == context.User.Id);

			await context.RespondAsync(test.Result.Content);
		}

		[Command("test3")]
		[Description("An embed test command!")]
		public async Task Test3(CommandContext context) {
			await context.TriggerTypingAsync();

			var color = new DiscordColor(12345);
			var embed = new DiscordEmbedBuilder {
				Color = color,
				Description = "THis is a test",
				Author = new DiscordEmbedBuilder.EmbedAuthor {
					Name = context.User.Username,
					IconUrl = context.User.AvatarUrl
				},
				Footer = new DiscordEmbedBuilder.EmbedFooter {
					Text = "There are so many thing you can add to embeds!"
				}
			};

			await context.RespondAsync(embed: embed);
		}

		[Command("owner")]
		[Description("An owner test command")]
		[RequireOwner]
		public async Task Owner(CommandContext context) {
			await context.RespondAsync("You are an owner of this bot!");
			var owners = context.Client.CurrentApplication.Owners;
			var id = context.User.Id;
			foreach (var owner in owners)
				await context.Guild.GetMemberAsync(id).Result.SendMessageAsync(owner.Username);
		}

		[Command("test4")]
		[Description("Another test command that does something you might not want to know")]
		[RequireOwner]
		public async Task Test4(CommandContext context) {
			await context.RespondAsync(context.User.Email);
		}

		[Command("serverStatus")]
		[Description("Check a Minecraft server status!")]
		public async Task ServerStatus(CommandContext context,
			[Description("The server's address (optional if there is a default server set)")] string address = null) {
			await context.TriggerTypingAsync();

			ServerStatus serverStatus;
			if (address == null) {
				try {
					serverStatus = MinecraftServerHelper.CheckServerStatus(context.Guild.Id);
				}
				catch (ServerNotFoundException) {
					await context.RespondAsync("You do not have a default server set!");
					return;
				}
			}
			else {
				serverStatus = MinecraftServerHelper.CheckServerStatus(address);
			}

			if (!serverStatus.WasSuccessful) {
				await context.RespondAsync(
					"Something happened while trying to get the server's status! Please try again later");
				return;
			}
			if (!serverStatus.IsValidWebsite) {
				await context.RespondAsync(
					"The address you provided is not valid! Please try with another link next time!");
				return;
			}

			await context.RespondAsync(embed: MinecraftServerHelper.BuildSmallDiscordEmbed(serverStatus));
		}

		[Command("setServer")]
		[Description("Use this command to set the default server to check using the serverStatus command.")]
		public async Task SetServer(CommandContext context, [Description("The default address you want to set")]
			string address) {
			var message = await context.RespondAsync("Checking server status...");
			var greenTick = DiscordEmoji.FromGuildEmote(context.Client, 661318582952787968);
			var redTick = DiscordEmoji.FromGuildEmote(context.Client, 661318582600466477);
			await context.TriggerTypingAsync();
			var interactivity = context.Client.GetInteractivity();
			var update = false;

			try {
				if (MinecraftServerHelper.CheckServerStatus(context.Guild.Id) != null) {
					await message.ModifyAsync(
						"This guild already has a default server set! Do you want to replace it?");
					await message.CreateReactionAsync(greenTick);
					await message.CreateReactionAsync(redTick);

					var wait = await interactivity.WaitForReactionAsync(
						emoteContext => emoteContext.Emoji == greenTick || emoteContext.Emoji == redTick, message,
						context.User);

					if (wait.Result != null) {
						await message.DeleteAllReactionsAsync();
						if (wait.Result.Emoji.Equals(greenTick)) {
							await message.ModifyAsync("Checking server status...");
							update = true;
						}
						else if (wait.Result.Emoji.Equals(redTick)) {
							await message.ModifyAsync("Request cancelled!");
							return;
						}
					}
					else {
						await message.ModifyAsync("Timed out! Request Cancelled.");
						return;
					}
				}
			}
			catch (ServerNotFoundException) { }

			var serverStatus = MinecraftServerHelper.CheckServerStatus(address);

			if (!serverStatus.WasSuccessful) {
				await message.ModifyAsync(
					"Something happened while trying to get the server's status! Please try again later");
				return;
			}

			if (!serverStatus.IsValidWebsite) {
				await message.ModifyAsync(
					"The address you provided is not valid! Please try with another link next time!");
				return;
			}

			if (!serverStatus.IsOnline) {
				await message.ModifyAsync("The server seems to be offline! Do you still want to add this server?");
				await message.CreateReactionAsync(greenTick);
				await message.CreateReactionAsync(redTick);

				var wait = await interactivity.WaitForReactionAsync(
					emoteContext => emoteContext.Emoji == greenTick || emoteContext.Emoji == redTick, message,
					context.User);

				if (wait.Result != null) {
					await message.DeleteAllReactionsAsync();
					if (wait.Result.Emoji.Equals(greenTick))
						await context.RespondAsync("Setting server...");
					else if (wait.Result.Emoji.Equals(redTick)) {
						await message.ModifyAsync("Request cancelled!");
						return;
					}
				}
				else {
					await message.ModifyAsync("Timed out! Request Cancelled.");
					return;
				}
			}

			MinecraftServerHelper.AddOrUpdateDefaultServer(context.Guild.Id, address);
			await message.ModifyAsync("Default server was " + (update ? "updated" : "added") + "!");
		}
	}

	[Group("bot")]
	[Aliases("self", "copebot")]
	[Description("Bot administration commands. Can only be executed by an owner of the bot!")]
	[RequireOwner]
	public class BotUserCommands : BaseCommandModule
	{
		[Command("presence")]
		[Description("Change the presence of the bot")]
		public async Task Presence(CommandContext context) {
			var interactivity = context.Client.GetInteractivity();


			
			await context.Client.UpdateStatusAsync(userStatus: UserStatus.Idle);
		}

		[Command("shutdown")]
		[Description("Shuts down the bot")]
		public async Task Shutdown(CommandContext context) {
			var interactivity = context.Client.GetInteractivity();

			await context.RespondAsync("Are you sure you want to shut down the bot?");
			
			var wait = interactivity.WaitForMessageAsync(waitContext =>
				waitContext.Author.Id == context.User.Id &&
				(waitContext.Content.Equals("yes", StringComparison.OrdinalIgnoreCase) ||
				 waitContext.Content.Equals("no", StringComparison.OrdinalIgnoreCase)));

			if (wait != null) {
				if (wait.Result.Result.Content.Equals("yes", StringComparison.OrdinalIgnoreCase)) {
					await context.RespondAsync("Goodbye!");
			
					context.Client.Dispose();
			
					Environment.Exit(0);
				}
				else if (wait.Result.Result.Content.Equals("no", StringComparison.OrdinalIgnoreCase))
					await context.RespondAsync("Shutdown cancelled");
			}
			else
				await context.RespondAsync("Timed out! Shutdown cancelled");

		}
	}
}