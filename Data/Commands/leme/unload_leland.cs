using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace amblflecasm.Data.Commands
{
	public class unload_leland : InteractionModuleBase<SocketInteractionContext>
	{
		[RequireLeme]
		[SlashCommand("unloadleland", "Remove le land slash commands", false, RunMode.Async)]
		public async Task UnloadLeLand()
		{
			try
			{
				string stringID = Program.GetConfigData("Servers", "le land").ToString();
				ulong ulongID = ulong.Parse(stringID);

				SocketGuild leland = this.Context.Client.Guilds.FirstOrDefault(guild => guild.Id == ulongID);

				if (leland == null || leland.Id != ulongID)
					throw new Exception();

				List<ApplicationCommandProperties> applicationCommandProperties = new List<ApplicationCommandProperties>();
				await leland.BulkOverwriteApplicationCommandAsync(applicationCommandProperties.ToArray());

				await this.RespondAsync("Successfully unregistered le land commands");
			} catch (Exception)
			{
				await this.RespondAsync("Failed to unregister le land commands");
			}
		}
	}
}
