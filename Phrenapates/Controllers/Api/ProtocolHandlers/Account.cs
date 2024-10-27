using Plana.Database;
using Plana.Database.ModelExtensions;
using Plana.FlatData;
using Plana.NetworkProtocol;
using Plana.Parcel;
using Phrenapates.Services;

namespace Phrenapates.Controllers.Api.ProtocolHandlers
{
    public class Account : ProtocolHandlerBase
    {
        private readonly ISessionKeyService sessionKeyService;
        private readonly SCHALEContext context;
        private readonly ExcelTableService excelTableService;

        public Account(
            IProtocolHandlerFactory protocolHandlerFactory,
            ISessionKeyService _sessionKeyService,
            SCHALEContext _context,
            ExcelTableService _excelTableService
        )
            : base(protocolHandlerFactory)
        {
            sessionKeyService = _sessionKeyService;
            context = _context;
            excelTableService = _excelTableService;
        }

        [ProtocolHandler(Protocol.Account_CheckYostar)]
        public ResponsePacket CheckYostarHandler(AccountCheckYostarRequest req)
        {
            string[] uidToken = req.EnterTicket.Split(':');

            var account = context.GuestAccounts.SingleOrDefault(x =>
                x.Uid == long.Parse(uidToken[0]) && x.Token == uidToken[1]
            );

            if (account is null)
            {
                return new AccountCheckYostarResponse()
                {
                    ResultState = 0,
                    ResultMessag = "Invalid account (EnterTicket, AccountCheckYostar)"
                };
            }

            return new AccountCheckYostarResponse()
            {
                ResultState = 1,
                SessionKey =
                    sessionKeyService.Create(account.Uid)
                    ?? new SessionKey() { MxToken = req.EnterTicket }
            };
        }

        [ProtocolHandler(Protocol.Account_Auth)]
        public ResponsePacket AuthHandler(AccountAuthRequest req)
        {
            if (req.SessionKey is null || req.SessionKey.AccountServerId == 0)
            {
                return new ErrorPacket() { ErrorCode = WebAPIErrorCode.AccountAuthNotCreated };
            }
            else
            {
                var account = sessionKeyService.GetAccount(req.SessionKey);

                return new AccountAuthResponse()
                {
                    CurrentVersion = req.Version,
                    AccountDB = account,
                    StaticOpenConditions = new()
                    {
                        { OpenConditionContent.Shop, OpenConditionLockReason.None },
                        { OpenConditionContent.Gacha, OpenConditionLockReason.None },
                        { OpenConditionContent.LobbyIllust, OpenConditionLockReason.None },
                        { OpenConditionContent.Raid, OpenConditionLockReason.StageClear },
                        { OpenConditionContent.Cafe, OpenConditionLockReason.StageClear },
                        { OpenConditionContent.Unit_Growth_Skill, OpenConditionLockReason.None },
                        { OpenConditionContent.Unit_Growth_LevelUp, OpenConditionLockReason.None },
                        {
                            OpenConditionContent.Unit_Growth_Transcendence,
                            OpenConditionLockReason.None
                        },
                        { OpenConditionContent.WeekDungeon, OpenConditionLockReason.StageClear },
                        { OpenConditionContent.Arena, OpenConditionLockReason.StageClear },
                        { OpenConditionContent.Academy, OpenConditionLockReason.StageClear },
                        { OpenConditionContent.Equip, OpenConditionLockReason.None },
                        { OpenConditionContent.Item, OpenConditionLockReason.None },
                        { OpenConditionContent.Mission, OpenConditionLockReason.None },
                        {
                            OpenConditionContent.WeekDungeon_Chase,
                            OpenConditionLockReason.StageClear
                        },
                        {
                            OpenConditionContent.__Deprecated_WeekDungeon_FindGift,
                            OpenConditionLockReason.None
                        },
                        {
                            OpenConditionContent.__Deprecated_WeekDungeon_Blood,
                            OpenConditionLockReason.None
                        },
                        { OpenConditionContent.Story_Sub, OpenConditionLockReason.None },
                        { OpenConditionContent.Story_Replay, OpenConditionLockReason.None },
                        { OpenConditionContent.None, OpenConditionLockReason.None },
                        { OpenConditionContent.Shop_Gem, OpenConditionLockReason.None },
                        { OpenConditionContent.Craft, OpenConditionLockReason.StageClear },
                        { OpenConditionContent.Student, OpenConditionLockReason.None },
                        { OpenConditionContent.GuideMission, OpenConditionLockReason.None },
                        { OpenConditionContent.Clan, OpenConditionLockReason.StageClear },
                        { OpenConditionContent.Echelon, OpenConditionLockReason.None },
                        { OpenConditionContent.Campaign, OpenConditionLockReason.None },
                        { OpenConditionContent.EventContent, OpenConditionLockReason.None },
                        { OpenConditionContent.EventStage_1, OpenConditionLockReason.StageClear },
                        { OpenConditionContent.EventStage_2, OpenConditionLockReason.None },
                        { OpenConditionContent.Talk, OpenConditionLockReason.None },
                        { OpenConditionContent.Billing, OpenConditionLockReason.None },
                        { OpenConditionContent.Schedule, OpenConditionLockReason.None },
                        { OpenConditionContent.Story, OpenConditionLockReason.None },
                        { OpenConditionContent.Tactic_Speed, OpenConditionLockReason.StageClear },
                        { OpenConditionContent.Cafe_Invite, OpenConditionLockReason.StageClear },
                        {
                            OpenConditionContent.Cafe_Invite_2,
                            OpenConditionLockReason.CafeRank | OpenConditionLockReason.StageClear
                        },
                        {
                            OpenConditionContent.EventMiniGame_1,
                            OpenConditionLockReason.StageClear
                        },
                        { OpenConditionContent.SchoolDungeon, OpenConditionLockReason.StageClear },
                        {
                            OpenConditionContent.TimeAttackDungeon,
                            OpenConditionLockReason.StageClear
                        },
                        { OpenConditionContent.ShiftingCraft, OpenConditionLockReason.StageClear },
                        { OpenConditionContent.Tactic_Skip, OpenConditionLockReason.StageClear },
                        { OpenConditionContent.Mulligan, OpenConditionLockReason.StageClear },
                        { OpenConditionContent.EventPermanent, OpenConditionLockReason.StageClear },
                        {
                            OpenConditionContent.Main_L_1_2,
                            OpenConditionLockReason.ScenarioModeClear
                        },
                        {
                            OpenConditionContent.Main_L_1_3,
                            OpenConditionLockReason.ScenarioModeClear
                        },
                        {
                            OpenConditionContent.Main_L_1_4,
                            OpenConditionLockReason.ScenarioModeClear
                        },
                        { OpenConditionContent.EliminateRaid, OpenConditionLockReason.StageClear },
                        { OpenConditionContent.Cafe_2, OpenConditionLockReason.StageClear },
                        { OpenConditionContent.MultiFloorRaid, OpenConditionLockReason.StageClear }
                    },
                    MissionProgressDBs =
                    [
                        .. context.MissionProgresses.Where(x =>
                            x.AccountServerId == account.ServerId
                        )
                    ]
                };
            }
        }

        [ProtocolHandler(Protocol.Account_Create)]
        public ResponsePacket CreateHandler(AccountCreateRequest req)
        {
            if (req.SessionKey is null)
            {
                return new ErrorPacket() { ErrorCode = WebAPIErrorCode.InvalidSession };
            }

            string[] uidToken = req.SessionKey.MxToken.Split(':');
            var account = new AccountDB(long.Parse(uidToken[0]));

            context.Accounts.Add(account);
            context.SaveChanges();

            // Default items
            context.Items.Add(
                new()
                {
                    AccountServerId = account.ServerId,
                    UniqueId = 2,
                    StackCount = 5
                }
            );

            // Default currencies
            var defaultCurrencies = excelTableService
                .GetTable<DefaultParcelExcelTable>()
                .UnPack()
                .DataList
                .Where(x => x.ParcelType == ParcelType.Currency)
                .ToList();

            AccountCurrencyDB accountCurrency = new()
            {
                AccountServerId = account.ServerId,
                AccountLevel = 1,
                AcademyLocationRankSum = 1,
                CurrencyDict = new()
            };

            foreach(var currencyType in Enum.GetValues(typeof(CurrencyTypes)).Cast<CurrencyTypes>())
            {
                if(currencyType == CurrencyTypes.Invalid)
                    continue;

                var amount = defaultCurrencies.Any(x => (CurrencyTypes)x.ParcelId == currencyType) ? defaultCurrencies.Where(x => (CurrencyTypes)x.ParcelId == currencyType).First().ParcelAmount : 0;
                accountCurrency.CurrencyDict.Add(currencyType, amount);
            }

            accountCurrency.UpdateTimeDict = new();
            foreach (var currencyType in Enum.GetValues(typeof(CurrencyTypes)).Cast<CurrencyTypes>())
            {
                if (currencyType == CurrencyTypes.Invalid)
                    continue;

                accountCurrency.UpdateTimeDict.Add(currencyType, DateTime.Now);
            }

            context.Currencies.Add(accountCurrency);

            // Default chars
            var defaultCharacters = excelTableService
                .GetTable<DefaultCharacterExcelTable>()
                .UnPack()
                .DataList;
            var newCharacters = defaultCharacters
                .Select(x =>
                {
                    var characterExcel = excelTableService
                        .GetTable<CharacterExcelTable>()
                        .UnPack()
                        .DataList.Find(y => y.Id == x.CharacterId);

                    return new CharacterDB()
                    {
                        UniqueId = x.CharacterId,
                        StarGrade = x.StarGrade,
                        Level = x.Level,
                        Exp = x.Exp,
                        FavorRank = x.FavorRank,
                        FavorExp = x.FavorExp,
                        PublicSkillLevel = 1,
                        ExSkillLevel = x.ExSkillLevel,
                        PassiveSkillLevel = x.PassiveSkillLevel,
                        ExtraPassiveSkillLevel = x.ExtraPassiveSkillLevel,
                        LeaderSkillLevel = x.LeaderSkillLevel,
                        IsNew = true,
                        IsLocked = true,
                        EquipmentServerIds = characterExcel is not null
                            ? characterExcel.EquipmentSlot.Select(x => (long)0).ToList()
                            : [0, 0, 0],
                        PotentialStats =
                        {
                            { 1, 0 },
                            { 2, 0 },
                            { 3, 0 }
                        }
                    };
                })
                .ToList();

            account.AddCharacters(context, [.. newCharacters]);
            context.SaveChanges();

            // Default Furniture
            var defaultFurnitureCafe = excelTableService
                .GetTable<DefaultFurnitureExcelTable>()
                .UnPack()
                .DataList;
            var cafeFurnitures = defaultFurnitureCafe.GetRange(0, 3).Select((x, index) => {
                return new FurnitureDB()
                {
                    CafeDBId = 0,
                    UniqueId = x.Id,
                    Location = x.Location,
                    PositionX = x.PositionX,
                    PositionY = x.PositionY,
                    Rotation = x.Rotation,
                    ItemDeploySequence = index + 1,
                    StackCount = 1
                };
            }).ToList();
            var secondCafeFurnitures = defaultFurnitureCafe.GetRange(0, 3).Select((x, index) => {
                return new FurnitureDB()
                {
                    CafeDBId = 1,
                    UniqueId = x.Id,
                    Location = x.Location,
                    PositionX = x.PositionX,
                    PositionY = x.PositionY,
                    Rotation = x.Rotation,
                    ItemDeploySequence = index + 4,
                    StackCount = 1
                };
            }).ToList();
            
            var combinedFurnitures = cafeFurnitures.Concat(secondCafeFurnitures).ToList();
            account.AddFurnitures(context, [.. combinedFurnitures]);
            context.SaveChanges();

            // Character Cafe
            var count = 0;
            Dictionary<long, CafeCharacterDB> CafeVisitCharacterDBs = [];
            foreach (var character in account.Characters)
            {
                CafeVisitCharacterDBs.Add(count, 
                    new CafeCharacterDB()
                    {
                        IsSummon = false,
                        UniqueId = character.UniqueId,
                        ServerId = character.ServerId,
                        LastInteractTime = DateTime.Now
                    }
                );
                count++;
            };

            // Cafe
            account.Cafes.Add(Cafe.CreateCafe(req.AccountId, account.Furnitures.ToList().GetRange(0, 3), CafeVisitCharacterDBs));
            account.Cafes.Add(Cafe.CreateSecondCafe(req.AccountId, account.Furnitures.ToList().GetRange(3, 3), CafeVisitCharacterDBs));
            context.SaveChanges();
            
            var favCharacter = defaultCharacters.Find(x => x.FavoriteCharacter);
            if (favCharacter is not null)
            {
                account.RepresentCharacterServerId = (int)
                    newCharacters.First(x => x.UniqueId == favCharacter.CharacterId).ServerId;
            }
            context.SaveChanges();
			
            // Mails
            var defaultMails = excelTableService.GetTable<DefaultMailExcelTable>().UnPack().DataList;
            var localizeExcel = excelTableService.GetExcelList<LocalizeExcel>("LocalizeDBSchema");

            foreach(var defaultMail in defaultMails)
            {
                List<ParcelInfo> parcelInfos = new();

                for(int i = 0; i < defaultMail.RewardParcelType.Count; i++)
                {
                    parcelInfos.Add(new()
                    {
                        Amount = defaultMail.RewardParcelAmount[i],
                        Key = new()
                        {
                            Type = defaultMail.RewardParcelType[i],
                            Id = defaultMail.RewardParcelId[i]
                        },
                        Multiplier = new(),
                        Probability = new()
                    });
                }

                account.Mails.Add(new()
                {
                    Type = defaultMail.MailType,
                    UniqueId = -1,
                    Sender = "プラナ",
                    Comment = localizeExcel.Where(x => x.Key == defaultMail.LocalizeCodeId).First().Jp,
                    SendDate = DateTime.Parse(defaultMail.MailSendPeriodFrom),
                    ExpireDate = DateTime.Parse(defaultMail.MailSendPeriodTo),
                    ParcelInfos = parcelInfos,
                    RemainParcelInfos = new()
                });
            }

            account.Mails.Add(Mail.CreateMail(req.AccountId));
            context.SaveChanges();

            return new AccountCreateResponse()
            {
                SessionKey = sessionKeyService.Create(account.PublisherAccountId)
            };
        }

        [ProtocolHandler(Protocol.Account_Nickname)]
        public ResponsePacket NicknameHandler(AccountNicknameRequest req)
        {
            var account = sessionKeyService.GetAccount(req.SessionKey);

            account.Nickname = req.Nickname;
            context.SaveChanges();

            return new AccountNicknameResponse() { AccountDB = account };
        }

        [ProtocolHandler(Protocol.Account_CallName)]
        public ResponsePacket CallNameHandler(AccountCallNameRequest req)
        {
            var account = sessionKeyService.GetAccount(req.SessionKey);

            account.CallName = req.CallName;
            context.SaveChanges();

            return new AccountCallNameResponse() { AccountDB = account };
        }

        [ProtocolHandler(Protocol.Account_LoginSync)]
        public ResponsePacket LoginSyncHandler(AccountLoginSyncRequest req)
        {
            var account = sessionKeyService.GetAccount(req.SessionKey);
            ArgumentNullException.ThrowIfNull(account);

            return new AccountLoginSyncResponse()
            {
                CafeGetInfoResponse = new CafeGetInfoResponse()
                {
                    CafeDB = account.Cafes.FirstOrDefault(x => x.AccountServerId == req.AccountId && x.CafeId == 1),
                    CafeDBs = account.Cafes.Where(x => x.AccountServerId == req.AccountId).ToList(),
                    FurnitureDBs = [.. account.Furnitures]
                },
                AccountCurrencySyncResponse = new AccountCurrencySyncResponse()
                {
                    AccountCurrencyDB = account.Currencies.FirstOrDefault()
                },
                CharacterListResponse = new CharacterListResponse()
                {
                    CharacterDBs = [.. account.Characters],
                    TSSCharacterDBs = [],
                    WeaponDBs = [.. account.Weapons],
                    CostumeDBs = [],
                },
                ItemListResponse = new ItemListResponse() { ItemDBs = [.. account.Items], },

                EchelonListResponse = new EchelonListResponse()
                {
                    EchelonDBs = [.. account.Echelons]
                },

                MemoryLobbyListResponse = new MemoryLobbyListResponse()
                {
                    MemoryLobbyDBs = [.. account.MemoryLobbies]
                },

                EventContentPermanentListResponse = new EventContentPermanentListResponse()
                {
                    PermanentDBs =
                    [
                        new() { EventContentId = 900801 },
                        new() { EventContentId = 900802 },
                        new() { EventContentId = 900803 },
                        new() { EventContentId = 900804 },
                        new() { EventContentId = 900805 },
                        new() { EventContentId = 900806 },
                        new() { EventContentId = 900808 },
                        new() { EventContentId = 900809 },
                        new() { EventContentId = 900810 },
                        new() { EventContentId = 900812 },
                        new() { EventContentId = 900813 },
                        new() { EventContentId = 900816 },
                        new() { EventContentId = 900701 },
                    ],
                },

                EquipmentItemListResponse = new EquipmentItemListResponse()
                {
                    EquipmentDBs = [.. account.Equipment]
                },

                CharacterGearListResponse = new CharacterGearListResponse()
                {
                    GearDBs = [.. account.Gears]
                },

                ClanLoginResponse = new ClanLoginResponse()
                {
                    AccountClanMemberDB = new() { AccountId = account.ServerId }
                },

                ScenarioListResponse = new ScenarioListResponse()
                {
                    ScenarioHistoryDBs = [.. account.Scenarios],
                    ScenarioGroupHistoryDBs = [.. account.ScenarioGroups]
                },

                EliminateRaidLoginResponse = new EliminateRaidLoginResponse()
                {
                    SeasonType = RaidSeasonType.Open,
                    SweepPointByRaidUniqueId = new()
                    {
                        { 2041104, int.MaxValue },
                        { 2041204, int.MaxValue }
                    }
                },

                FriendCode = "SUS",
            };
        }

        [ProtocolHandler(Protocol.Account_GetTutorial)]
        public ResponsePacket GetTutorialHandler(AccountGetTutorialRequest req)
        {
            var tutorialIds = context
                .AccountTutorials.SingleOrDefault(x =>
                    x.AccountServerId == sessionKeyService.GetAccount(req.SessionKey).ServerId
                )
                ?.TutorialIds;

            return new AccountGetTutorialResponse()
            {
                TutorialIds = tutorialIds ?? Enumerable.Range(1, 27).Select(i => (long)i).ToList()
            };
        }

        [ProtocolHandler(Protocol.Account_SetTutorial)]
        public ResponsePacket SetTutorialHandler(AccountSetTutorialRequest req)
        {
            var account = sessionKeyService.GetAccount(req.SessionKey);
            var tutorial = context.AccountTutorials.SingleOrDefault(x =>
                x.AccountServerId == account.ServerId
            );
            if (tutorial == null)
            {
                tutorial = new()
                {
                    AccountServerId = account.ServerId,
                    TutorialIds = req.TutorialIds
                };
                context.AccountTutorials.Add(tutorial);
            }
            else
            {
                tutorial.TutorialIds = req.TutorialIds;
            }
            context.SaveChanges();

            return new AccountSetTutorialResponse();
        }


        [ProtocolHandler(Protocol.Account_SetRepresentCharacterAndComment)]
        public ResponsePacket SetRepresentCharacterAndCommentHandler(AccountSetRepresentCharacterAndCommentRequest req)
        {
            var account = sessionKeyService.GetAccount(req.SessionKey);

            account.RepresentCharacterServerId = req.RepresentCharacterServerId;
            account.Comment = req.Comment;

            context.SaveChanges();

            return new AccountSetRepresentCharacterAndCommentResponse()
            {
                AccountDB = account,
                RepresentCharacterDB = account.Characters.FirstOrDefault(x => x.ServerId == req.RepresentCharacterServerId)
            };
        }

        // TODO: others handlers, move to different handler group later
        [ProtocolHandler(Protocol.NetworkTime_Sync)]
        public ResponsePacket NetworkTime_SyncHandler(NetworkTimeSyncRequest req)
        {
            return new NetworkTimeSyncResponse()
            {
                ReceiveTick = DateTimeOffset.Now.Ticks,
                EchoSendTick = DateTimeOffset.Now.Ticks
            };
        }

        [ProtocolHandler(Protocol.ContentSave_Get)]
        public ResponsePacket ContentSave_GetHandler(ContentSaveGetRequest req)
        {
            return new ContentSaveGetResponse();
        }

        [ProtocolHandler(Protocol.Toast_List)]
        public ResponsePacket ToastListHandler(ToastListRequest req)
        {
            return new ToastListResponse();
        }

        [ProtocolHandler(Protocol.Event_RewardIncrease)]
        public ResponsePacket Event_RewardIncreaseHandler(EventRewardIncreaseRequest req)
        {
            return new EventRewardIncreaseResponse();
        }

        [ProtocolHandler(Protocol.OpenCondition_EventList)]
        public ResponsePacket OpenCondition_EventListHandler(OpenConditionEventListRequest req)
        {
            return new OpenConditionEventListResponse()
            {
                // all open for now ig
                StaticOpenConditions = Enum.GetValues(typeof(OpenConditionContent))
                    .Cast<OpenConditionContent>()
                    .ToDictionary(key => key, key => OpenConditionLockReason.None)
            };
        }

        [ProtocolHandler(Protocol.Notification_EventContentReddotCheck)]
        public ResponsePacket Notification_EventContentReddotCheckHandler(
            NotificationEventContentReddotRequest req
        )
        {
            return new NotificationEventContentReddotResponse();
        }

        [ProtocolHandler(Protocol.Billing_PurchaseListByYostar)]
        public ResponsePacket Billing_PurchaseListByYostarHandler(
            BillingPurchaseListByYostarRequest req
        )
        {
            return new BillingPurchaseListByYostarResponse();
        }

        [ProtocolHandler(Protocol.MiniGame_MissionList)]
        public ResponsePacket MiniGame_MissionListHandler(MiniGameMissionListRequest req)
        {
            return new MiniGameMissionListResponse();
        }

        [ProtocolHandler(Protocol.Attachment_EmblemAcquire)]
        public ResponsePacket Attachment_EmblemAcquireHandler(AttachmentEmblemAcquireRequest req)
        {
            return new AttachmentEmblemAcquireResponse();
        }

        [ProtocolHandler(Protocol.Friend_List)]
        public ResponsePacket Attachment_EmblemAcquireHandler(FriendListRequest req)
        {
            return new FriendListResponse();
        }

        [ProtocolHandler(Protocol.Friend_GetIdCard)]
        public ResponsePacket Friend_GetIdCardHandler(FriendGetIdCardRequest req)
        {
            return new FriendGetIdCardResponse();
        }

        [ProtocolHandler(Protocol.Account_InvalidateToken)]
        public ResponsePacket InvalidateTokenHandler(AccountInvalidateTokenRequest req)
        {
            return new AccountInvalidateTokenResponse();
        }
    }
}
