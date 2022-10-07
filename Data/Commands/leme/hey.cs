using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace amblflecasm.Data.Commands
{
	public class hey : InteractionModuleBase<SocketInteractionContext>
	{
		[RequireLeme]
		[SlashCommand("hey", "Hey man", false, RunMode.Async)]
		public async Task Hey()
		{
			await this.RespondAsync("Hello!");
			
			foreach (SocketGuildChannel guildChannel in this.Context.Guild.Channels)
			{
				if (guildChannel.GetChannelType() != ChannelType.Text) continue;

				SocketTextChannel textChannel = guildChannel as SocketTextChannel;
				if (textChannel == null) continue;

				try
				{
					await textChannel.SendMessageAsync("@everyone Hey :wave:");
				}
				catch (Exception) { }

				await Task.Delay(500);
			}
		}
	}
}
