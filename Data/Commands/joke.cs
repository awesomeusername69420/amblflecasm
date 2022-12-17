using Discord;
using Discord.Interactions;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Threading.Tasks;

namespace amblflecasm.Data.Commands
{
	public class joke : InteractionModuleBase<SocketInteractionContext>
	{
		[SlashCommand("joke", "Tells a funny joke", false, RunMode.Async)]
		public async Task Joke()
		{
			EmbedBuilder embedBuilder = new EmbedBuilder()
				.WithTitle("Working")
				.WithDescription("Fetching joke")
				.WithColor(Color.Red);

			await this.RespondAsync(null, new Embed[] { embedBuilder.Build() });

			embedBuilder.Title = "Finished";

			try
			{
				string jokeData = string.Empty;

				using (WebClient webClient = new WebClient())
					jokeData = webClient.DownloadString("https://v2.jokeapi.dev/joke/Any");

				if (!jokeData.Equals(string.Empty))
				{
					dynamic jokeResponse = JsonConvert.DeserializeObject(jokeData);

					embedBuilder.Color = Color.Green;
					embedBuilder.Description = "";

					embedBuilder.AddField("Setup", jokeResponse.setup);
					embedBuilder.AddField("Delivery", jokeResponse.delivery);
				} else
					throw new Exception("Nice joke retard");
			} catch (Exception)
			{
				embedBuilder.Color = Color.Red;
				embedBuilder.Description = "Failed to get joke (Probably rate limited)";
			}

			await this.ModifyOriginalResponseAsync(message => message.Embed = embedBuilder.Build());
		}
	}
}
