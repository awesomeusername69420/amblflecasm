using Discord;
using Discord.Interactions;
using IPinfo;
using IPinfo.Models;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Threading.Tasks;

namespace amblflecasm.Data.Commands
{
	public class ipinfo : InteractionModuleBase<SocketInteractionContext>
	{
		IPinfoClient ipinfoClient;

		[SlashCommand("ipinfo", "Get various information about an IP Address", false, RunMode.Async)]
		public async Task IPInfo(string ipAddress)
		{
			EmbedBuilder embedBuilder = new EmbedBuilder()
				.WithTitle("Working")
				.WithDescription("Fetching IP Address information")
				.WithColor(Color.Red);

			await this.RespondAsync(null, new Embed[] { embedBuilder.Build() });

			embedBuilder.Title = "Finished";

			IPAddress ip;
			if (!IPAddress.TryParse(ipAddress, out ip))
			{
				embedBuilder.Description = "Invalid IP Address";

				await this.ModifyOriginalResponseAsync(message => message.Embed = embedBuilder.Build());

				return;
			}

			ipAddress = ip.ToString();

			if (ipinfoClient == null)
				try
				{
					ipinfoClient = new IPinfoClient.Builder()
						.AccessToken(Program.GetConfigData("Tokens", "IPinfo").ToString())
						.Build();
				}
				catch (Exception)
				{
					embedBuilder.Description = "Failed to build IPInfo client";

					await this.ModifyOriginalResponseAsync(message => message.Embed = embedBuilder.Build());

					return;
				}

			try
			{
				IPResponse ipinfoResponse = await ipinfoClient.IPApi.GetDetailsAsync(ipAddress);

				if (ipinfoResponse.Bogon)
				{
					embedBuilder.Color = Color.Orange;
					embedBuilder.Description = "This IP Address is a Bogon";
				} else
				{
					// IPinfo.io

					embedBuilder.Color = Color.Green;
					embedBuilder.Description = "";

					embedBuilder.AddField("IPinfo Data", string.Format(@"
Response IP: `{0}`
Hostname: `{1}`
City: `{2}`
Region: `{3}`
Country: `{4}`
Timezone: `{5}`
Loc: `{6}`
Org: `{7}`",

					ipinfoResponse.IP ?? "Unknown",
					ipinfoResponse.Hostname ?? "Unknown",
					ipinfoResponse.City ?? "Unknown",
					ipinfoResponse.Region ?? "Unknown",
					ipinfoResponse.CountryName ?? "Unknown",
					ipinfoResponse.Timezone ?? "Unknown",
					ipinfoResponse.Loc ?? "Unknown",
					ipinfoResponse.Org ?? "Unknown"));

					// ProxyCheck.io

					string proxyCheckData = string.Empty;

					using (WebClient webClient = new WebClient())
						proxyCheckData = webClient.DownloadString("https://proxycheck.io/v2/" + ipAddress + "&vpn=1"); // API keys are for pussies (Seriously though, what's up with the lack of a requirement?"

					if (!proxyCheckData.Equals(string.Empty))
					{
						dynamic proxyCheckResponse = JsonConvert.DeserializeObject(proxyCheckData);

						embedBuilder.AddField("ProxyCheck Data", string.Format(@"
Is Proxy: `{0}`
Proxy Type: `{1}`
Proxy Operator: `{2}`
Operator Website: `{3}`",

						proxyCheckResponse[ipAddress]?.proxy ?? "Unknown",
						proxyCheckResponse[ipAddress]?.type ?? "Unknown",
						proxyCheckResponse[ipAddress]?["operator"]?.name ?? "Unknown",
						proxyCheckResponse[ipAddress]?["operator"]?.url ?? "Unknown"));
					} else
						embedBuilder.AddField("ProxyCheck Data", "Failed to parse IP");
				}

				embedBuilder.Footer = new EmbedFooterBuilder()
				{
					Text = string.Format("IP Address information for '{0}'", ipAddress)
				};

				await this.ModifyOriginalResponseAsync(message => message.Embed = embedBuilder.Build());
			} catch (Exception)
			{
				embedBuilder.Color = Color.Red;
				embedBuilder.Description = "Failed to parse IP Address";
			
				await this.ModifyOriginalResponseAsync(message => message.Embed = embedBuilder.Build());
			}
		}
	}
}