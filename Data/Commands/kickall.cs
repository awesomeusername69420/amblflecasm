using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace amblflecasm.Data.Commands
{
	public class kickall : InteractionModuleBase<SocketInteractionContext>
	{
		[SlashCommand("kickall", "Kicks all non-mods and non-admins", false, RunMode.Async)]
		public async Task KickAll(bool ignoreBots = true)
		{
			IGuildUser guildUser = this.Context.User as IGuildUser;
			if (guildUser == null)
			{
				await this.RespondAsync("How did you manage this");
				return;
			}

			if (!guildUser.GuildPermissions.Administrator && !guildUser.GuildPermissions.KickMembers)
			{
				await this.RespondAsync(Program.GetRandomDenial());
				return;
			}

			EmbedBuilder embedBuilder = new EmbedBuilder()
				.WithTitle("Working")
				.WithDescription("Searching for users")
				.WithColor(Color.Red);

			await this.RespondAsync(null, new Embed[] { embedBuilder.Build() });

			embedBuilder.Title = "Finished";

			List<SocketGuildUser> guildUsers = new List<SocketGuildUser>();
			foreach (SocketGuildUser socketGuildUser in this.Context.Guild.Users)
				if (socketGuildUser.GuildPermissions.Administrator || socketGuildUser.GuildPermissions.ModerateMembers || socketGuildUser.GuildPermissions.KickMembers || socketGuildUser.GuildPermissions.BanMembers || (socketGuildUser.IsBot && ignoreBots))
					continue;
				else
					guildUsers.Add(socketGuildUser);

			if (guildUsers.Count < 1)
			{
				embedBuilder.Description = "No members found";

				await this.ModifyOriginalResponseAsync(message => message.Embed = embedBuilder.Build());

				return;
			}

			embedBuilder.Description = string.Format("Attempting to kick {0} {1}", guildUsers.Count, Program.AutoPlural("member", guildUsers.Count));

			await this.ModifyOriginalResponseAsync(message => message.Embed = embedBuilder.Build());

			string reason = string.Format("KickAll by {0}", guildUser.Id);

			int kicked = 0;
			foreach (SocketGuildUser socketGuildUser in guildUsers)
				try
				{
					await socketGuildUser.KickAsync(reason);
					kicked++;
				}
				catch (Exception) { }

			if (kicked > 0)
				if (kicked == guildUsers.Count)
					embedBuilder.Color = Color.Green;
				else
					embedBuilder.Color = Color.Orange;
			else
				embedBuilder.Color = Color.Red;

			embedBuilder.Description = string.Format("Kicked {0} {1}", kicked, Program.AutoPlural("member", kicked));

			await this.ModifyOriginalResponseAsync(message => message.Embed = embedBuilder.Build());
		}
	}
}
