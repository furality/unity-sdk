﻿using System;

namespace Furality.SDK.External.Auth
{
    [Serializable]
    public class TokenResponse
    {
        public string access_token;
        public string refresh_token;
        public string id_token;
        public string token_type;
        public int expires_in;
    }
}