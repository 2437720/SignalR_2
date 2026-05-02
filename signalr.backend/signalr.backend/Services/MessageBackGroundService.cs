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



        public MessageBackGroundService(IServiceScopeFactory serviceScopeFactory, IHubContext<ChatHub> chatHub)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _chatHub = chatHub; 
            
        }

        public async Task ChannelLePlusPopulaire(CancellationToken stoppingToken)
        {

            using (IServiceScope scope = _serviceScopeFactory.CreateScope())
            {

                ApplicationDbContext context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                Channel? channelLePlusPopulaire = await context.Channel.Where(c => c.NbMessages == context.Channel.Max(c => c.NbMessages) && c.NbMessages > 0).FirstOrDefaultAsync();

               
                
                List<Channel>? channelsLePlusPopulaires = await context.Channel.Where(c => c.NbMessages == channelLePlusPopulaire.NbMessages).ToListAsync();

                
               
                foreach(Channel channel in channelsLePlusPopulaires)
                {
                    string groupName = "Channel" + channel.Id;

                    if(channel.NbMessages > 0)
                    {
                        _chatHub.Clients.Group(groupName).SendAsync("NbMessages", channel.NbMessages, stoppingToken);
                    }     

                    
                }



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
