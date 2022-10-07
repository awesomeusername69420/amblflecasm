using Discord;
using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace amblflecasm.Data.Commands
{
	public class help : InteractionModuleBase<SocketInteractionContext>
	{
		[SlashCommand("help", "Help I'm retarded", false, RunMode.Async)]
		public async Task Help()
		{
			EmbedBuilder embedBuilder = new EmbedBuilder()
				.WithTitle("Command List")
				.WithColor(Color.Blue);

			List<SlashCommandInfo> commands = new List<SlashCommandInfo>(Program.interactionService.SlashCommands); // Can't sort the actual list because it's read only
			commands.Sort((a, b) => a.Name.CompareTo(b.Name));

			foreach (SlashCommandInfo commandInfo in commands)
				embedBuilder.AddField(commandInfo.Name, commandInfo.Description);

			await this.RespondAsync(null, new Embed[] { embedBuilder.Build() });
		}
	}
}
