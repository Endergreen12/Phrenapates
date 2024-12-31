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
                var excelDump = Path.Combine(resourceDir, "Dumped/Excel");
                var excelDbDump = Path.Combine(resourceDir,"Dumped/ExcelDB");
                var excelDir = Path.Combine(resourceDir, "Excel");
                var excelDbDir = Path.Combine(resourceDir, "ExcelDB.db");
                if(!Directory.Exists(excelDump) && !Directory.Exists(excelDbDump) && Directory.Exists(excelDir) && File.Exists(excelDbDir))
                {
                    Directory.CreateDirectory(excelDump);
                    Directory.CreateDirectory(excelDbDump);
                    TableService.DumpExcels(excelDir, excelDbDir, excelDump);   
                    TableService.DumpExcelDB(excelDbDir, excelDbDump);
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

        /*public List<T> GetExcelDB<T>(string schema = "", bool bypassCache = false)
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
                        .Invoke(null, new object[] { new ByteBuffer((byte[])reader[0]) }));
                }
            }

            caches[type] = excelList!;
            logger.LogDebug("{Excel} loaded and cached", type.Name);

            return excelList;
        }*/

        public List<T> GetExcelDB<T>() where T : struct, IFlatbufferObject
        {
            var excelList = new List<T>();
            var type = typeof(T);

            string schema = type.Name.Replace("Excel", "DBSchema");

            using (var dbConnection = new SQLiteConnection($"Data Source = {Path.Join(resourceDir, "ExcelDB.db")}"))
            {
                dbConnection.Open();
                var command = dbConnection.CreateCommand();
                command.CommandText = $"SELECT Bytes FROM {schema}";
                
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var byteBuffer = new ByteBuffer((byte[])reader["Bytes"]);
                    var flatBufferObject = (T)type.GetMethod($"GetRootAs{type.Name}", BindingFlags.Static | BindingFlags.Public, [typeof(ByteBuffer)])!
                        .Invoke(null, new object[] { byteBuffer });
                    
                    excelList.Add(flatBufferObject);
                }
            }

            logger.LogDebug("{Schema} data loaded", schema);
            return excelList;
        }

        public T GetExcelDBID<T>(object id) where T : struct, IFlatbufferObject
        {
            var type = typeof(T);
            string schema = type.Name.Replace("Excel", "DBSchema");

            var dbSchemaType = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .FirstOrDefault(t => t.Name == schema);
            
            if(dbSchemaType == null) throw new InvalidOperationException($"No properties found on type {dbSchemaType.Name}.");

            var identifierProperty = dbSchemaType
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .FirstOrDefault()
                ?? throw new InvalidOperationException($"No properties found on type {dbSchemaType.Name}.");

            var identifierName = identifierProperty.Name;

            T result = default(T);

            using (var dbConnection = new SQLiteConnection($"Data Source = {Path.Join(resourceDir, "ExcelDB.db")}"))
            {
                dbConnection.Open();
                var command = dbConnection.CreateCommand();
                command.CommandText = $"SELECT Bytes FROM {schema} WHERE {identifierName} = @Id";
                command.Parameters.AddWithValue("@Id", id);

                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    var byteBuffer = new ByteBuffer((byte[])reader["Bytes"]);
                    var getRootMethod = type.GetMethod($"GetRootAs{type.Name}", BindingFlags.Static | BindingFlags.Public, [typeof(ByteBuffer)])
                    ?? throw new InvalidOperationException($"Method GetRootAs{type.Name} not found for type {type.Name}");

                    result = (T)getRootMethod.Invoke(null, new object[] { byteBuffer });
                }
            }

            logger.LogDebug("{Schema} data with {IdName} = {IdValue} loaded", schema, identifierName, id);

            return result;
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
