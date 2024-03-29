﻿using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RestSharp;

namespace Revolt
{
    public class RevoltClientSelf
    {
        public RevoltClient Client { get; }

        public RevoltClientSelf(RevoltClient client)
        {
            this.Client = client;
        }

        public Task EditProfileAsync(UserInfo info)
            => Client._requestAsync($"{Client.ApiUrl}/users/@me", Method.PATCH, JsonConvert.SerializeObject(info));

        // todo: edit username and passwd thing route
    }

    public class UserInfo
    {
        [JsonProperty("status")] public Status Status;
        [JsonProperty("profile")] public Profile Profile;
    }

    public class Status
    {
        [JsonProperty("text")] public string Text;

        // todo: presence enum
        [JsonProperty("presence")] public string Presence;
    }

    public class Profile
    {
        [JsonIgnore]
        public string? Content
        {
            get => content;
            set
            {
                if (value?.Length >= ContentMaxLength)
                    throw new InvalidOperationException("sussus");
                content = value;
            }
        }

        [JsonProperty("content")] private string? content;
        public const int ContentMaxLength = 2000;
        [JsonProperty("background")] public Attachment? Background { get; internal set; }
    }
}