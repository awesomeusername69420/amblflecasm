using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace Discord.Interactions
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
	public class RequireLemeAttribute : PreconditionAttribute
	{
		public override async Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider serviceProvider)
		{
			if (amblflecasm.Program.IsUserLeme(context.User as SocketUser))
				return PreconditionResult.FromSuccess();
			else
				return PreconditionResult.FromError("Not leme");
		}
	}
}
