using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace amblflecasm.Data.Commands
{
	public class egg : InteractionModuleBase<SocketInteractionContext>
	{
		[SlashCommand("egg", "Egg someone", false, RunMode.Async)]
		public async Task Egg(SocketUser target)
		{
			IGuildUser guildTarget = target as IGuildUser;
			if (guildTarget == null)
			{
				await this.RespondAsync("How did you manage this");
				return;
			}

			await this.RespondAsync("<@" + guildTarget.Id + "> :egg: :egg: :egg: :egg: :egg: :egg: :egg: :egg: :egg: :egg: :egg: :egg: :egg: :egg: :egg:");
		}
	}
}
