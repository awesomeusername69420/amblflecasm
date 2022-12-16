using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace amblflecasm.Data.Commands
{
	public class ban : InteractionModuleBase<SocketInteractionContext>
	{
		[SlashCommand("ban", "Ban some noob", false, RunMode.Async)]
		public async Task KickAll(SocketUser target, string Reason = "")
		{
			IGuildUser guildUser = this.Context.User as IGuildUser;
			if (guildUser == null)
			{
				await this.RespondAsync("How did you manage this");
				return;
			}

			if (!guildUser.GuildPermissions.Administrator && !guildUser.GuildPermissions.BanMembers)
			{
				await this.RespondAsync(Program.GetRandomDenial());
				return;
			}

			IGuildUser guildTarget = target as IGuildUser;
			if (guildTarget == null)
			{
				await this.RespondAsync("How did you manage this");
				return;
			}

			if (Reason.Length < 1)
				Reason = "Owned";

			EmbedBuilder embedBuilder = new EmbedBuilder()
				.WithTitle("Working")
				.WithDescription("Attempting to ban member")
				.WithColor(Color.Red);

			await this.RespondAsync(null, new Embed[] { embedBuilder.Build() });

			embedBuilder.Title = "Finished";

			try
			{
				await guildTarget.BanAsync(0, Reason);

				embedBuilder.Description = String.Format("Banned `{0}` ({1}) Reason: `{2}`", guildTarget.Username + "#" + guildTarget.Discriminator, guildTarget.Id, Reason);
			} catch (Exception)
			{
				embedBuilder.Description = "Failed to ban member";
			}

			await this.ModifyOriginalResponseAsync(message => message.Embed = embedBuilder.Build());
		}
	}
}
