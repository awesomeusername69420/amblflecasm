using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace amblflecasm.Data.Commands
{
	public class unloadguild : InteractionModuleBase<SocketInteractionContext>
	{
		[SlashCommand("unloadguild", "Remove slash commands from this guild", false, RunMode.Async)]
		public async Task UnloadGuild()
		{
			IGuildUser guildUser = this.Context.User as IGuildUser;
			if (guildUser == null)
			{
				await this.RespondAsync("How did you manage this");
				return;
			}

			if (!guildUser.GuildPermissions.Administrator)
			{
				await this.RespondAsync(Program.GetRandomDenial());
				return;
			}

			try
			{
				SocketGuild socketGuild = this.Context.Guild as SocketGuild;

				if (socketGuild == null)
					throw new Exception();

				List<ApplicationCommandProperties> applicationCommandProperties = new List<ApplicationCommandProperties>();
				await socketGuild.BulkOverwriteApplicationCommandAsync(applicationCommandProperties.ToArray());

				await this.RespondAsync("Successfully unregistered slash commands for this guild");
			} catch (Exception)
			{
				await this.RespondAsync("Failed to unregister slash commands");
			}
		}
	}
}
