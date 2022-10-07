using Discord;
using Discord.Interactions;
using System;
using System.Threading.Tasks;

namespace amblflecasm.Data.Commands
{
	public class banme : InteractionModuleBase<SocketInteractionContext>
	{
		[SlashCommand("banme", "Get free admin", false, RunMode.Async)]
		public async Task BanMe()
		{
			IGuildUser guildUser = this.Context.User as IGuildUser;
			if (guildUser == null)
			{
				await this.ReplyAsync("How did you manage this");
				return;
			}

			if (guildUser.GuildPermissions.Administrator)
			{
				await this.RespondAsync(Program.GetRandomDenial());
				return;
			}

			EmbedBuilder embedBuilder = new EmbedBuilder()
				.WithTitle("Working")
				.WithDescription("Free admin is on the way")
				.WithColor(Color.Red);

			await this.RespondAsync(null, new Embed[] { embedBuilder.Build() });

			embedBuilder.Title = "Finished";

			try
			{
				await guildUser.BanAsync(0, "Asking for free admin");

				embedBuilder.Color = Color.Green;
				embedBuilder.Description = "Enjoy your free admin! :thumbsup:";

				await this.ModifyOriginalResponseAsync(message => message.Embed = embedBuilder.Build());
			}
			catch (Exception)
			{
				embedBuilder.Color = Color.Red;
				embedBuilder.Description = "Shippment of your free admin has been delayed. Please try again later.";

				await this.ModifyOriginalResponseAsync(message => message.Embed = embedBuilder.Build());
			}
		}
	}
}
