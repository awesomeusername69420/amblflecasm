using Discord;
using Discord.Interactions;
using System;
using System.Threading.Tasks;

namespace amblflecasm.Data.Commands
{
	public class poof : InteractionModuleBase<SocketInteractionContext>
	{
		[RequireUser("Stormy")]
		[SlashCommand("poof", "What poof do you have of that?", false, RunMode.Async)]
		public async Task Poof()
		{
			IGuildUser guildUser = this.Context.User as IGuildUser;
			if (guildUser == null)
			{
				await this.RespondAsync("How did you manage this");
				return;
			}

			try
			{
				await guildUser.SendFileAsync(Program.GetConfigData("FilePaths", "poof").ToString());
				await this.RespondAsync(":D");
			} catch (Exception)
			{
				await this.RespondAsync("D:");
			}
		}
	}
}
