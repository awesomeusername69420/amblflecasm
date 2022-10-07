using Discord;
using Discord.Interactions;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace amblflecasm.Data.Commands
{
	public class ping : InteractionModuleBase<SocketInteractionContext>
	{
		[SlashCommand("ping", "pong", false, RunMode.Async)]
		public async Task Ping()
		{
			EmbedBuilder embedBuilder = new EmbedBuilder()
				.WithColor(Color.Green);

			embedBuilder.AddField("Gateway Latency", this.Context.Client.Latency + " ms");
			embedBuilder.AddField("Interaction Latency", DateTimeOffset.UtcNow.Subtract(this.Context.Interaction.CreatedAt).Milliseconds + " ms");

			await this.RespondAsync(null, new Embed[] { embedBuilder.Build() });
		}
	}
}
