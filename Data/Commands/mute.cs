using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace amblflecasm.Data.Commands
{
	public class mute : InteractionModuleBase<SocketInteractionContext>
	{
		[SlashCommand("mute", "Time someone out for a long time", false, RunMode.Async)]
		public async Task Mute(SocketUser target, int hours = 648)
		{
			IGuildUser guildUser = this.Context.User as IGuildUser;
			IGuildUser guildTarget = target as IGuildUser;
			if (guildUser == null || guildTarget == null)
			{
				await this.RespondAsync("How did you manage this");
				return;
			}

			if ((!guildUser.GuildPermissions.Administrator && !guildUser.GuildPermissions.ModerateMembers) || (guildTarget.GuildPermissions.Administrator || guildTarget.GuildPermissions.ModerateMembers))
			{
				await this.RespondAsync(Program.GetRandomDenial());
				return;
			}

			if (hours < 1)
			{
				await this.RespondAsync("What kinda time frame is that");
				return;
			}

			EmbedBuilder embedBuilder = new EmbedBuilder()
				.WithTitle("Working")
				.WithDescription("Attempting to timeout member")
				.WithColor(Color.Red);

			await this.RespondAsync(null, new Embed[] { embedBuilder.Build() });

			embedBuilder.Title = "Finished";

			try
			{
				await guildTarget.SetTimeOutAsync(new TimeSpan(hours, 0, 0));

				embedBuilder.Color = Color.Green;
				embedBuilder.Description = string.Format("Member timed out for {0} {1}", hours, Program.AutoPlural("hour", hours));

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
