using Discord;
using Discord.Interactions;
using Steam.Models.SteamCommunity;
using SteamWebAPI2.Interfaces;
using SteamWebAPI2.Models;
using SteamWebAPI2.Utilities;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace amblflecasm.Data.Commands
{
	public class steam : InteractionModuleBase<SocketInteractionContext>
	{
		SteamWebInterfaceFactory webInterfaceFactory;
		SteamUser steamWebInterface;

		private static string BadAvatar = "https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/b5/b5bd56c1aa4644a474a2e4972be27ef9e82e517e_full.jpg"; // Default '?'

		[SlashCommand("steam", "Steam API lookup", false, RunMode.Async)]
		public async Task Steam(string token)
		{
			EmbedBuilder embedBuilder = new EmbedBuilder()
				.WithTitle("Working")
				.WithDescription("Fetching Steam data")
				.WithColor(Color.Red);

			await this.RespondAsync(null, new Embed[] { embedBuilder.Build() });

			embedBuilder.Title = "Finished";

			if (webInterfaceFactory == null)
				try
				{
					webInterfaceFactory = new SteamWebInterfaceFactory(Program.GetConfigData("Tokens", "Steam").ToString());
				}
				catch (Exception)
				{
					embedBuilder.Description = "Failed to create web interface factory";

					await this.ModifyOriginalResponseAsync(message => message.Embed = embedBuilder.Build());

					return;
				}

			if (steamWebInterface == null)
				try
				{
					steamWebInterface = webInterfaceFactory.CreateSteamWebInterface<SteamUser>(new HttpClient());
				}
				catch (Exception)
				{
					embedBuilder.Description = "Failed to create Steam interface factory";

					await this.ModifyOriginalResponseAsync(message => message.Embed = embedBuilder.Build());

					return;
				}

			try
			{
				SteamId steamIDObject = null;

				try
				{
					string steamID = string.Copy(token);

					if (steamID.Contains(":"))
					{
						if (steamID.Contains("["))
							steamID = steamID.Replace("[", string.Empty).Replace("]", string.Empty); // Why does this one have brackets??

						string[] steamIDSplit = steamID.Split(':');
						string accountIDString = steamIDSplit[steamIDSplit.Length - 1]; // Account ID is the last part

						uint accountID = uint.Parse(accountIDString);

						if (steamID.Contains("STEAM_")) // Regular Steam ID's need to be doubled for whatever reason
							accountID = accountID * 2;

						steamIDObject = new SteamId(accountID);
					}
					else // Assume 64
						steamIDObject = new SteamId(ulong.Parse(steamID));
				} catch (Exception) // Maybe it's a custom url?
				{
					ISteamWebResponse<ulong> vanityResponse = await steamWebInterface.ResolveVanityUrlAsync(token);
					steamIDObject = new SteamId(ulong.Parse(vanityResponse.Data.ToString()));
				}

				if (steamIDObject == null)
					throw new Exception();

				ISteamWebResponse<PlayerSummaryModel> playerSummaryResponse = await steamWebInterface.GetPlayerSummaryAsync(steamIDObject.To64Bit());
				
				if (playerSummaryResponse.Data == null)
					throw new Exception();

				embedBuilder.Color = Color.Green;
				embedBuilder.Description = "";

				embedBuilder.AddField("Steam Data", string.Format(@"
Nickname: `{0}`
Real Name: `{1}`
Profile URL: {2}
Date Created: `{3}`
Status: `{4}`",
				
				playerSummaryResponse.Data.Nickname ?? "Unknown",
				playerSummaryResponse.Data.RealName ?? "Unknown",
				playerSummaryResponse.Data.ProfileUrl ?? "Unknown",
				playerSummaryResponse.Data.AccountCreatedDate.ToString(),
				playerSummaryResponse.Data.UserStatus.ToString()));

				embedBuilder.ImageUrl = playerSummaryResponse.Data.AvatarFullUrl ?? playerSummaryResponse.Data.AvatarMediumUrl ?? playerSummaryResponse.Data.AvatarUrl;

				if (embedBuilder.ImageUrl == null) // Anti 404
					embedBuilder.ImageUrl = BadAvatar;
				else
					try
					{
						HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create(embedBuilder.ImageUrl); // For some reason this errors on 404 not sure why

						if (((HttpWebResponse)webRequest.GetResponse()).StatusCode == HttpStatusCode.NotFound)
							embedBuilder.ImageUrl = BadAvatar;
					}
					catch (Exception)
					{
						embedBuilder.ImageUrl = BadAvatar;
					}

				embedBuilder.Footer = new EmbedFooterBuilder()
				{
					Text = string.Format("SteamID information for '{0}' / '{1}'", steamIDObject.ToLegacyFormat(), steamIDObject.To64Bit())
				};

				await this.ModifyOriginalResponseAsync(message => message.Embed = embedBuilder.Build());
			} catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());

				embedBuilder.Color = Color.Red;
				embedBuilder.Description = "Failed to parse Steam data";

				await this.ModifyOriginalResponseAsync(message => message.Embed = embedBuilder.Build());
			}
		}
	}
}
