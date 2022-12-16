using Discord;
using Discord.Interactions;
using System;
using System.Threading.Tasks;

namespace amblflecasm.Data.Commands
{
	public class makechannels : InteractionModuleBase<SocketInteractionContext>
	{
		[RequireUser("leme")]
		[SlashCommand("makechannels", "Make some random channels", false, RunMode.Async)]
		public async Task MakeChannels(int amount = 100)
		{
			if (amount < 1)
			{
				await this.RespondAsync("How many?");
				return;
			}

			EmbedBuilder embedBuilder = new EmbedBuilder()
				.WithTitle("Working")
				.WithDescription(string.Format("Attempting to make {0} {1}", amount, Program.AutoPlural("channel", amount)))
				.WithColor(Color.Red);

			await this.RespondAsync(null, new Embed[] { embedBuilder.Build() });

			embedBuilder.Title = "Finished";

			int created = 0;
			for (int i = 1; i <= amount; i++)
			{
				try
				{
					await this.Context.Guild.CreateTextChannelAsync(Program.GetRandomString().ToLower()); // Text channels can't have uppercase names anyway
					created++;
				}
				catch (Exception) { }

				await Task.Delay(500);
			}

			if (created > 0)
				if (created == amount)
					embedBuilder.Color = Color.Green;
				else
					embedBuilder.Color = Color.Orange;
			else
				embedBuilder.Color = Color.Red;

			embedBuilder.Description = string.Format("Created {0} {1}", created, Program.AutoPlural("channel", created));

			await this.ModifyOriginalResponseAsync(message => message.Embed = embedBuilder.Build());
		}
	}
}
