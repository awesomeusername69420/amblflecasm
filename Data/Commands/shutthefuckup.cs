using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace amblflecasm.Data.Commands
{
	public class shutthefuckup : InteractionModuleBase<SocketInteractionContext>
	{
		[SlashCommand("shutthefuckup", "Times out everyone", false, RunMode.Async)]
		public async Task ShutTheFuckUp(int minutes = 2)
		{
			IGuildUser guildUser = this.Context.User as IGuildUser;
			if (guildUser == null)
			{
				await this.RespondAsync("How did you manage this");
				return;
			}

			if (!guildUser.GuildPermissions.Administrator && !guildUser.GuildPermissions.ModerateMembers)
			{
				try
				{
					await guildUser.SetTimeOutAsync(new TimeSpan(0, 2, 0)); // Don't be too mean
				}
				catch (Exception) { }
				
				await this.RespondAsync("You shut up");
				return;
			}

			IReadOnlyCollection<SocketGuildUser> guildUsers = this.Context.Guild.Users;

			EmbedBuilder embedBuilder = new EmbedBuilder()
				.WithTitle("Working")
				.WithColor(Color.Red);

			if (minutes < 1)
			{
				await this.RespondAsync("What kinda time frame is that");
				return;
			}

			TimeSpan timeSpan = new TimeSpan(0, minutes, 0);

			embedBuilder.Description = string.Format("Attempting to time out {0} {1}...", guildUsers.Count, Program.AutoPlural("member", guildUsers.Count));
			await this.RespondAsync(null, new Embed[] { embedBuilder.Build() });

			int success = 0;
			foreach (IGuildUser user in guildUsers)
			{
				if (user.IsBot || user.GuildPermissions.Administrator || user.Id == guildUser.Id) continue;

				try
				{
					await user.SetTimeOutAsync(timeSpan);
					success = success + 1;
				}
				catch (Exception) { }

				await Task.Delay(250);
			}

			embedBuilder.Title = "Finished";

			if (success > 0)
			{
				embedBuilder.Color = Color.Green;
				embedBuilder.Description = string.Format("Timed out {0} {1} for {2} {3}", success, Program.AutoPlural("member", success), minutes, Program.AutoPlural("minute", minutes));
			} else
			{
				embedBuilder.Color = Color.Red;
				embedBuilder.Description = "Timed out nobody";
			}

			await this.ModifyOriginalResponseAsync(message => message.Embed = embedBuilder.Build());
		}
	}
}
