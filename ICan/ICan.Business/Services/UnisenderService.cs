using ICan.Common;
using ICan.Common.Domain;
using ICan.Common.Models;
using ICan.Common.Models.Enums;
using ICan.Common.Models.Exceptions;
using ICan.Common.Models.Unisender;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ICan.Business.Services
{
	public class UnisenderService: IEmailService
	{
		private readonly UnisenderClient _unisenderClient;
		
		public UnisenderService(UnisenderClient unisenderClient)
		{
			_unisenderClient = unisenderClient;
		}

		public async Task<int> ExportContacts(IEnumerable<ApplicationUser> contacts)
		{
			if (contacts == null || !contacts.Any())
			{
				return 0;
			}

			var spList = await GetList(nameof(ClientType.JointPurchase));
			if(spList == null)
			{
				spList = await CreateList(nameof(ClientType.JointPurchase));
			}

			var shopList = await GetList(nameof(ClientType.Shop));
			if (shopList == null)
			{
				shopList = await CreateList(nameof(ClientType.Shop));
			}

			var shopsContacts = contacts.Where(x => x.ClientType == (int)ClientType.Shop).ToArray();
			var spContacts = contacts.Where(x => x.ClientType == (int)ClientType.JointPurchase).ToArray();

			var totalRegistred = 0;
			if (shopsContacts.Any())
			{
				var subscribeShopContactsResult = await _unisenderClient.Subscribe(shopList.Id, shopsContacts);
				totalRegistred += subscribeShopContactsResult.Result.Inserted;
			}

			if (spContacts.Any())
			{
				var subscribeSPContactsResult = await _unisenderClient.Subscribe(spList.Id, spContacts);
				totalRegistred += subscribeSPContactsResult.Result.Inserted;
			}

			return totalRegistred;
		}

		public async Task<string> PrepareCampaign(OptCampaign campaign)
		{
			var lists = await _unisenderClient.GetLists();
			if (!lists.Any())
			{
				throw new UserException("Необходимо сначала экспортировать контакты");
			}

			var listId = GetListId(campaign.CampaignType, lists);
			var createdMessageId = await _unisenderClient.CreateMessage(listId, campaign);
			return createdMessageId.ToString();
		}

		public async Task SendCampaign(string externalCampaignId)
		{
			await _unisenderClient.SendCampaign(externalCampaignId);
		}

		public async Task AddClient(ApplicationUser client)
		{
			var lists = await _unisenderClient.GetLists();
			if (!lists.Any())
			{
				return;
			}

			var listId = GetListId(client.ClientType, lists);
			await _unisenderClient.Subscribe(listId, new[] { client });
		}

		public async Task RemoveClient(ApplicationUser client)
		{
			var lists = await _unisenderClient.GetLists();
			if (!lists.Any())
			{
				return;
			}

			await _unisenderClient.Exclude(lists.Select(x => x.Id).ToArray(), client);
		}

		public async Task UpdateClient(ApplicationUser client)
		{
			var lists = await _unisenderClient.GetLists();
			if (!lists.Any())
			{
				return;
			}

			await _unisenderClient.Exclude(lists.Select(x => x.Id).ToArray(), client);
			var listId = GetListId(client.ClientType, lists);
			await _unisenderClient.Subscribe(listId, new[] { client });
		}

		private async Task<UnisenderList> GetList(string listName)
		{
			var lists = await _unisenderClient.GetLists();
			if (lists.Any())
			{
				var list = lists.FirstOrDefault(x => x.Title.Equals(listName));
				if(list != null)
				{
					return list;
				}
			}

			return null;
		}

		private static int GetListId(int campaignType, IEnumerable<UnisenderList> lists)
		{
			if(campaignType == (int)CampaignType.Shop)
			{
				return lists.First(x => x.Title.Equals(nameof(ClientType.Shop))).Id;
			}

			return lists.First(x => x.Title.Equals(nameof(ClientType.JointPurchase))).Id;
		}

		private async Task<UnisenderList> CreateList(string listName)
		{
			var createResult = await _unisenderClient.CreateList<ResultId>(listName);
			
			if(createResult.Result != null)
			{
				return new UnisenderList 
				{ 
					Id = createResult.Result.Id,
					Title = listName
				};
			}

			throw new UserException("Не удалось создать новый список");
		}
	}
}
