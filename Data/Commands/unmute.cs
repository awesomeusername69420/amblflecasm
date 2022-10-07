using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace amblflecasm.Data.Commands
{
	public class unmute : InteractionModuleBase<SocketInteractionContext>
	{
		[SlashCommand("unmute", "Un-timeout someone", false, RunMode.Async)]
		public async Task UnMute(SocketUser target)
		{
			IGuildUser guildUser = this.Context.User as IGuildUser;
			IGuildUser guildTarget = target as IGuildUser;
			if (guildUser == null || guildTarget == null)
			{
				await this.RespondAsync("How did you manage this");
				return;
			}

			if (!guildUser.GuildPermissions.Administrator && !guildUser.GuildPermissions.ModerateMembers)
			{
				await this.RespondAsync(Program.GetRandomDenial());
				return;
			}

			if (guildTarget.TimedOutUntil == null)
			{
				await this.RespondAsync("That person isn't timed out dumbfuck");
				return;
			}

			EmbedBuilder embedBuilder = new EmbedBuilder()
				.WithTitle("Working")
				.WithDescription("Attempting to un-timeout member")
				.WithColor(Color.Red);

			await this.RespondAsync(null, new Embed[] { embedBuilder.Build() });

			embedBuilder.Title = "Finished";

			try
			{
				await guildTarget.RemoveTimeOutAsync();

				embedBuilder.Color = Color.Green;
				embedBuilder.Description = "Member un-timed out";

				await this.ModifyOriginalResponseAsync(message => message.Embed = embedBuilder.Build());
			} catch (Exception)
			{
				embedBuilder.Color = Color.Red;
				embedBuilder.Description = "Epic fail";

				await this.ModifyOriginalResponseAsync(message => message.Embed = embedBuilder.Build());
			}
		}
	}
}
