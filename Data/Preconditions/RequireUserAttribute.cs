using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace Discord.Interactions
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
	public class RequireUserAttribute : PreconditionAttribute
	{
		private string user;

		public RequireUserAttribute(string user)
		{
			this.user = user;
		}

		public override async Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider serviceProvider)
		{
			if (amblflecasm.Program.IsUser(context.User as SocketUser, this.user))
				return PreconditionResult.FromSuccess();
			else
			{
				try
				{
					await context.Interaction.RespondAsync(this.user + " only");
				}
				catch (Exception) { }

				return PreconditionResult.FromError("Not " + this.user);
			}
		}
	}
}
