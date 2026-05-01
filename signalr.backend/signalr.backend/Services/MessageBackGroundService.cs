using Microsoft.AspNetCore.SignalR;
using signalr.backend.Hubs;
using signalr.backend.Data;
using signalr.backend.Models;
using Microsoft.EntityFrameworkCore;

namespace signalr.backend.Services
{
    public class MessageBackGroundService : BackgroundService
    {

        public const int DELAY = 30 * 1000; // 30 secondes

        protected  IServiceScopeFactory _serviceScopeFactory;

        private IHubContext<ChatHub> _chatHub;

        public MessageBackGroundService(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;

            
        }

        public async Task ChannelLePlusPopulaire(CancellationToken stoppingToken)
        {

            using (IServiceScope scope = _serviceScopeFactory.CreateScope())
            {
                ApplicationDbContext context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                Channel? channelLePlusPopulaire = await context.Channel.Where(c => c.NbMessages == context.Channel.Max(c => c.NbMessages)).FirstOrDefaultAsync();
                
                List<Channel>? channelsLePlusPopulaires = await context.Channel.Where(c => c.NbMessages == channelLePlusPopulaire.NbMessages).ToListAsync();



            }



            return;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(DELAY, stoppingToken);
                await ChannelLePlusPopulaire(stoppingToken);
                
            }
        }
    }
}
