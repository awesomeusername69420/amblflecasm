using Discord;
using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace amblflecasm.Data.Commands
{
	public class help : InteractionModuleBase<SocketInteractionContext>
	{
		[SlashCommand("help", "Help I'm retarded", false, RunMode.Async)]
		public async Task Help()
		{
			EmbedBuilder embedBuilder = new EmbedBuilder()
				.WithTitle("Working")
				.WithDescription("Building help text")
				.WithColor(Color.Red);

			await this.RespondAsync(null, new Embed[] { embedBuilder.Build() });

			embedBuilder.Title = "Finished";

			try // Maybe back this up somehow so it doesn't need regenerated every time?
			{
				List<string> displayCommands = new List<string>();

				foreach (SlashCommandInfo commandInfo in Program.interactionService.SlashCommands)
				{
					foreach (PreconditionAttribute pa in commandInfo.Preconditions) // Don't show user specific commands
						if (pa.GetType() == typeof(RequireUserAttribute))
							goto BREAKOUTER;

					ModuleInfo moduleInfo = commandInfo.Module;
					string commandName = string.Empty;

					if (moduleInfo != null)
					{
						while (moduleInfo.IsSubModule) // Try to find parent module
						{
							commandName = moduleInfo.Name + " " + commandName; // Add current module's command to the comand name
							moduleInfo = moduleInfo.Parent;
						}

						commandName = moduleInfo.Name + " " + commandName; // Finalize

						if (!commandInfo.Name.Equals(commandName.Trim())) // Add trailing command name
							commandName = commandName + commandInfo.Name;

						commandName = commandName.Trim();
					}

					foreach (SlashCommandParameterInfo parameterInfo in commandInfo.Parameters) // Show the parameters ( [] = required, () = optional )
						commandName = commandName + (parameterInfo.IsRequired ? " [" : " (") + parameterInfo.Name + (parameterInfo.IsRequired ? "]" : ")");

					displayCommands.Add("• " + commandName + " - " + commandInfo.Description);

				BREAKOUTER:;
				}

				displayCommands.Sort();

				embedBuilder.AddField("Commands ( [] = Required; () = Optional )", "```" + string.Join("\n", displayCommands.ToArray()) + "```"); // Pop it!

				embedBuilder.Color = Color.Green;
				embedBuilder.Description = "";
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
				embedBuilder.Color = Color.Red;
				embedBuilder.Description = "Failed to build help text";
			}

			await this.ModifyOriginalResponseAsync(message => message.Embed = embedBuilder.Build());
		}
	}
}
