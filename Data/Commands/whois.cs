using Discord;
using Discord.Interactions;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Whois;

namespace amblflecasm.Data.Commands
{
	public class whois : InteractionModuleBase<SocketInteractionContext>
	{
		WhoisLookup client;

		private bool IsValidDomain(string domain)
		{
			return Uri.CheckHostName(domain) != UriHostNameType.Unknown;
		}

		[SlashCommand("whois", "WhoIs lookup on a domain", false, RunMode.Async)]
		public async Task WhoIs(string domain)
		{
			EmbedBuilder embedBuilder = new EmbedBuilder()
				.WithTitle("Working")
				.WithDescription("Fetching WhoIs information")
				.WithColor(Color.Red);

			await this.RespondAsync(null, new Embed[] { embedBuilder.Build() });

			embedBuilder.Title = "Finished";

			if (!IsValidDomain(domain))
			{
				embedBuilder.Description = "Invalid domain provided";

				await this.ModifyOriginalResponseAsync(message => message.Embed = embedBuilder.Build());

				return;
			}

			if (client == null)
				try
				{
					client = new WhoisLookup();
				} catch (Exception)
				{
					embedBuilder.Description = "Failed to build WhoIs client";

					await this.ModifyOriginalResponseAsync(message => message.Embed = embedBuilder.Build());

					return;
				}

			try
			{
				WhoisResponse response = await client.LookupAsync(domain);

				embedBuilder.AddField("WhoIs Data", string.Format(@"
WhoIs Server: `{0}`
Registered: `{1}`
Updated: `{2}`
Expires: `{3}`
Domain: `{4}`",
				response.WhoisServer?.ToString() ?? "Unknown",
				response.Registered?.ToString() ?? "Unknown",
				response.Updated?.ToString() ?? "Unknown",
				response.Expiration?.ToString() ?? "Unknown",
				response.DomainName?.ToString() ?? "Unknown"));

				string address = "Unknown";
				if (response.Registrant.Address != null)
					address = string.Join(" ", response.Registrant.Address.ToArray<string>());

				embedBuilder.AddField("Registrant Data", string.Format(@"
Name: `{0}`
Address: `{1}`
Email: `{2}`
Phone Number: `{3}` (Extension: `{4}`)
Organization: `{5}`
Updated: `{6}`",

				response.Registrant?.Name ?? "Unknown",
				address,
				response.Registrant?.Email ?? "Unknown",
				response.Registrant?.TelephoneNumber ?? "Unknown",
				response.Registrant?.TelephoneNumberExt ?? "None",
				response.Registrant?.Organization ?? "Unknown",
				response.Registrant?.Updated?.ToString() ?? "Unknown"));

				embedBuilder.Color = Color.Green;
				embedBuilder.Description = "";

				embedBuilder.Footer = new EmbedFooterBuilder()
				{
					Text = string.Format("WhoIs information for '{0}'", domain)
				};

				await this.ModifyOriginalResponseAsync(message => message.Embed = embedBuilder.Build());
			} catch (Exception)
			{
				embedBuilder.Color = Color.Red;
				embedBuilder.Description = "Failed to parse WhoIs data";

				await this.ModifyOriginalResponseAsync(message => message.Embed = embedBuilder.Build());
			}
		}
	}
}
