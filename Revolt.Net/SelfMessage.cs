﻿using System.Threading.Tasks;

namespace Revolt
{
    public class SelfMessage : Message
    {
        public Task EditAsync(string newContent)
            => Client.Channels.EditMessageAsync(ChannelId, _id, newContent);
    }
}