using System.Data.SQLite;
using System.Reflection;
using Google.FlatBuffers;
using Ionic.Zip;
using Plana.Crypto;
using Phrenapates.Utils;
using Serilog;

namespace Phrenapates.Services
{
    // TODO: High priority, cache UnPack-ed table!
    public class ExcelTableService(ILogger<ExcelTableService> _logger)
    {
        private readonly ILogger<ExcelTableService> logger = _logger;
        private readonly Dictionary<Type, object> caches = [];
        public static string resourceDir = Path.Join(Path.GetDirectoryName(AppContext.BaseDirectory), "Resources");

        public static async Task LoadResources()
        {
            var versionTxtPath = Path.Combine(resourceDir, "version.txt");

#if DEBUG
                var dumpDir = Path.Join(Path.GetDirectoryName(AppContext.BaseDirectory), "dumped");
                var excelDir = Path.Combine(resourceDir, "Excel");
                var excelDbDir = Path.Join(resourceDir, "ExcelDB.db");
                if(!Directory.Exists(dumpDir) && Directory.Exists(excelDir) && File.Exists(excelDbDir))
                {
                    Directory.CreateDirectory(dumpDir);
                    TableService.DumpExcels(excelDir, excelDbDir, dumpDir);   
                }
#endif

            if (Directory.Exists(resourceDir))
            {
                if(File.Exists(versionTxtPath) && File.ReadAllText(versionTxtPath) == Config.Instance.VersionId)
                {
                    Log.Information("Resources already downloaded, skipping...");
                    return;
                } else {
                    Directory.Delete(resourceDir, true);
                    Log.Information("The version of the resource is different from that of the server and the resource will be downloaded again");
                }
            }

            Log.Information("Downloading resources, this may take a while...");

            Directory.CreateDirectory(resourceDir);

            var baseUrl = $"https://prod-clientpatch.bluearchiveyostar.com/{Config.Instance.VersionId}/TableBundles/";
            var tableCatalogName = "TableCatalog.bytes";
            var tableCatalogUrl = baseUrl + tableCatalogName;
            var tableCatalogPath = Path.Combine(resourceDir, tableCatalogName);

            var client = new HttpClient();
            client.Timeout = TimeSpan.FromMinutes(10);
            var downloadList = new List<string>() { "ExcelDB.db", "Excel.zip" };

            var downloadedFolderName = "downloaded";
            var downloadedFolderPath = Path.Combine(resourceDir, downloadedFolderName);
            if (!Directory.Exists(downloadedFolderPath))
            {
                Directory.CreateDirectory(downloadedFolderPath);
            }

            foreach (var bundle in downloadList)
            {
                var downloadFileName = bundle;
                var downloadUrl = baseUrl + downloadFileName;
                var downloadFilePath = Path.Combine(downloadedFolderPath, downloadFileName);
                Log.Information($"Downloading {downloadFileName}...");
                File.WriteAllBytes(downloadFilePath, await client.GetByteArrayAsync(downloadUrl));

                if(Path.GetExtension(downloadFilePath) == ".zip")
                {
                    Log.Information($"Extracting {downloadFileName}...");
                    using (var zip = ZipFile.Read(downloadFilePath))
                    {
                        zip.Password = Convert.ToBase64String(TableService.CreatePassword(Path.GetFileName(downloadFilePath)));
                        zip.ExtractAll(Path.Combine(resourceDir, Path.GetFileNameWithoutExtension(downloadFilePath)), ExtractExistingFileAction.OverwriteSilently);
                    }
                } else
                {
                    File.Move(downloadFilePath, Path.Combine(resourceDir, downloadFileName));
                }
            }

            Log.Information($"Deleting {downloadedFolderName} folder...");
            Directory.Delete(downloadedFolderPath, true);

            File.WriteAllText(versionTxtPath, Config.Instance.VersionId);

            Log.Information($"Resource Version {Config.Instance.VersionId} downloaded!");
        }

        /// <summary>
        /// Please <b>only</b> use this to get table that <b>have a respective file</b> (i.e. <c>CharacterExcelTable</c> have <c>characterexceltable.bytes</c>)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        public T GetTable<T>(bool bypassCache = false) where T : IFlatbufferObject
        {
            var type = typeof(T);

            if (!bypassCache && caches.TryGetValue(type, out var cache))
                return (T)cache;

            var excelDir = Path.Combine(resourceDir, "Excel");
            var bytesFilePath = Path.Join(excelDir, $"{type.Name.ToLower()}.bytes");
            if (!File.Exists(bytesFilePath))
            {
                throw new FileNotFoundException($"bytes files for {type.Name} not found");
            }

            var bytes = File.ReadAllBytes(bytesFilePath);
            TableEncryptionService.XOR(type.Name, bytes);
            var inst = type.GetMethod($"GetRootAs{type.Name}", BindingFlags.Static | BindingFlags.Public, [typeof(ByteBuffer)])!.Invoke(null, [new ByteBuffer(bytes)]);

            caches.Add(type, inst!);
            logger.LogDebug("{Excel} loaded and cached", type.Name);

            return (T)inst!;
        }

        public List<T> GetExcelList<T>(string schema, bool bypassCache = false)
        {
            var excelList = new List<T>();
            var type = typeof(T);

            if (!bypassCache && caches.TryGetValue(type, out var cache))
                return (List<T>)cache;

            using (var dbConnection = new SQLiteConnection($"Data Source = {Path.Join(resourceDir, "ExcelDB.db")}"))
            {
                dbConnection.Open();
                var command = dbConnection.CreateCommand();
                command.CommandText = $"SELECT Bytes FROM {schema}";
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    excelList.Add((T)type.GetMethod($"GetRootAs{type.Name}", BindingFlags.Static | BindingFlags.Public, [typeof(ByteBuffer)])!
                        .Invoke(null, [new ByteBuffer((byte[])reader[0])]));
                }
            }

            caches[type] = excelList!;
            logger.LogDebug("{Excel} loaded and cached", type.Name);

            return excelList;
        }
    }

    internal static class ExcelTableServiceExtensions
    {
        public static void AddExcelTableService(this IServiceCollection services)
        {
            services.AddSingleton<ExcelTableService>();
        }
    }
}
