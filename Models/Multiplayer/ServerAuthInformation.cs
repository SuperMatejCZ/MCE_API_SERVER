﻿using System;

namespace MCE_API_SERVER.Models.Multiplayer
{
    public enum ServerAuthInformation
    {
        NotAuthed,
        AuthStage1,
        AuthStage2,
        Authed,
        FailedAuth
    }

    public class ServerInformation
    {
        public Guid serverId { get; set; }
        public string ip { get; set; }
        public int port { get; set; }
    }
}
