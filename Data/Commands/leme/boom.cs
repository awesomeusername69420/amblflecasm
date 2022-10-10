using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace amblflecasm.Data.Commands
{
	public class boom : InteractionModuleBase<SocketInteractionContext>
	{
		[RequireLeme]
		[SlashCommand("boom", "Vine BOOM", false, RunMode.Async)]
		public async Task Boom(SocketUser target, int amount = 10)
		{
			IGuildUser guildTarget = target as IGuildUser;
			if (guildTarget == null)
			{
				await this.RespondAsync("How did you manage this");
				return;
			}

			if (amount < 1)
			{
				await this.RespondAsync("How many times?");
				return;
			}

			EmbedBuilder embedBuilder = new EmbedBuilder()
				.WithTitle("Working")
				.WithDescription(string.Format("Booming {0} {1} {2}", guildTarget.Username, amount, Program.AutoPlural("time", amount)))
				.WithColor(Color.Red);
			
			await this.RespondAsync(null, new Embed[] { embedBuilder.Build() });

			int booms = 0;
			for (int i = 1; i <= amount; i++, booms++)
				try
				{
					await guildTarget.SendMessageAsync("https://tenor.com/view/vineboom-ilybeeduo-gif-23126674");
					await Task.Delay(750);
				}
				catch (Exception)
				{
					break;
				}

			embedBuilder.Title = "Finished";

			if (booms > 0)
				if (booms == amount)
					embedBuilder.Color = Color.Green;
				else
					embedBuilder.Color = Color.Orange;
			else
				embedBuilder.Color = Color.Red;

			embedBuilder.Description = string.Format("Boomed {0} {1} {2}", guildTarget.Username, booms, Program.AutoPlural("time", booms));

			await this.ModifyOriginalResponseAsync(message => message.Embed = embedBuilder.Build());
		}
	}
}
