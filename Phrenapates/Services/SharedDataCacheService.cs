using Plana.FlatData;
using Plana.Utils;

namespace Phrenapates.Services
{
    public class SharedDataCacheService
    {
        private readonly ILogger<SharedDataCacheService> _logger;
        private readonly ExcelTableService _excelTable;
        private readonly List<CharacterExcelT> _charaList;
        private readonly List<CharacterExcelT> _charaListR;
        private readonly List<CharacterExcelT> _charaListSR;
        private readonly List<CharacterExcelT> _charaListSSR;
        private readonly List<CharacterExcelT> _charaListRNormal;
        private readonly List<CharacterExcelT> _charaListSRNormal;
        private readonly List<CharacterExcelT> _charaListSSRNormal;
        private readonly List<CharacterExcelT> _charaListUnique;
        private readonly List<CharacterExcelT> _charaListEvent;

        public IReadOnlyList<CharacterExcelT> CharaList => _charaList;
        public IReadOnlyList<CharacterExcelT> CharaListR => _charaListR;
        public IReadOnlyList<CharacterExcelT> CharaListSR => _charaListSR;
        public IReadOnlyList<CharacterExcelT> CharaListSSR => _charaListSSR;
        public IReadOnlyList<CharacterExcelT> CharaListRNormal=> _charaListRNormal;
        public IReadOnlyList<CharacterExcelT> CharaListSRNormal=> _charaListSRNormal;
        public IReadOnlyList<CharacterExcelT> CharaListSSRNormal=> _charaListSSRNormal;
        public IReadOnlyList<CharacterExcelT> CharaListUnique => _charaListUnique;
        public IReadOnlyList<CharacterExcelT> CharaListEvent => _charaListEvent;

        public SharedDataCacheService(ILogger<SharedDataCacheService> logger, ExcelTableService excelTable)
        {
            _logger = logger;
            _excelTable = excelTable;

            _charaList = excelTable
                .GetTable<CharacterExcelTable>()
                .UnPack()
                .DataList!
                .Where(x => x is
                {
                    IsPlayable: true,
                    IsPlayableCharacter: true,
                    IsDummy: false,
                    IsNPC: false,
                    ProductionStep_: ProductionStep.Release,
                })
                .ToList();
            _charaListR = _charaList.Where(x => x.Rarity_ == Rarity.R).ToList();
            _charaListSR = _charaList.Where(x => x.Rarity_ == Rarity.SR).ToList();
            _charaListSSR = _charaList.Where(x => x.Rarity_ == Rarity.SSR).ToList();
            _charaListRNormal = _charaListR.Where(x => x.GetStudentType() == StudentType.Normal).ToList();
            _charaListSRNormal = _charaListSR.Where(x => x.GetStudentType() == StudentType.Normal).ToList();
            _charaListSSRNormal = _charaListSSR.Where(x => x.GetStudentType() == StudentType.Normal).ToList();
            _charaListUnique = _charaListR.Where(x => x.GetStudentType() == StudentType.Unique).ToList();
            _charaListEvent = _charaListR.Where(x => x.GetStudentType() == StudentType.Event).ToList();
        }
    }

    internal static class DataCacheServiceExtensions
    {
        public static void AddSharedDataCache(this IServiceCollection services)
        {
            services.AddSingleton<SharedDataCacheService>();
        }
    }
}
