using System.Net;
using Plana.Database;

namespace Phrenapates.Services.Irc
{
    public class IrcService : BackgroundService
    {
        private IrcServer server;

        public IrcService(ILogger<IrcService> _logger, SCHALEContext _context, ExcelTableService excelTableService)
        {
            server = new IrcServer(IPAddress.Any, 6667, _logger, _context, excelTableService);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await server.StartAsync(stoppingToken);
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            server.Stop();
            await base.StopAsync(stoppingToken);
        }
    }

    internal static class IrcServiceExtensions
    {
        public static void AddIrcService(this IServiceCollection services)
        {
            services.AddHostedService<IrcService>();
        }
    }
}
