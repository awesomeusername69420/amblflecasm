using Discord;
using Discord.Interactions;
using System;
using System.Text;
using System.Threading.Tasks;

namespace amblflecasm.Data.Commands
{
	[Group("base64", "Base64 operations")]
	public class base64 : InteractionModuleBase<SocketInteractionContext>
	{
		[SlashCommand("encode", "Base64 encode", false, RunMode.Async)]
		public async Task Base64Encode(string text)
		{
			EmbedBuilder embedBuilder = new EmbedBuilder()
				.WithTitle("Working")
				.WithDescription("Encoding")
				.WithColor(Color.Red);
			
			await this.RespondAsync(null, new Embed[] { embedBuilder.Build() });

			embedBuilder.Title = "Finished";

			try
			{
				byte[] bytes = Encoding.UTF8.GetBytes(text);

				embedBuilder.Color = Color.Green;
				embedBuilder.Description = string.Format("`{0}`", Convert.ToBase64String(bytes));
			} catch (Exception) // Should never happen, but just in case something goes horribly wrong
			{
				embedBuilder.Color = Color.Red;
				embedBuilder.Description = "Failed to convert string";
			}

			await this.ModifyOriginalResponseAsync(message => message.Embed = embedBuilder.Build());
		}

		[SlashCommand("decode", "Base64 decode", false, RunMode.Async)]
		public async Task Base64Decode(string text)
		{
			EmbedBuilder embedBuilder = new EmbedBuilder()
				.WithTitle("Working")
				.WithDescription("Decoding")
				.WithColor(Color.Red);

			await this.RespondAsync(null, new Embed[] { embedBuilder.Build() });

			embedBuilder.Title = "Finished";

			try
			{
				byte[] bytes = Convert.FromBase64String(text);

				embedBuilder.Color = Color.Green;
				embedBuilder.Description = string.Format("`{0}`", Encoding.UTF8.GetString(bytes));
			}
			catch (Exception)
			{
				embedBuilder.Color = Color.Red;
				embedBuilder.Description = "Failed to convert string";
			}

			await this.ModifyOriginalResponseAsync(message => message.Embed = embedBuilder.Build());
		}
	}
}
