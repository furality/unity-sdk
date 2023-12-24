﻿using Furality.SDK.External.Api.Models.User;

namespace Furality.SDK.External.Api.Endpoints
{
    public class FoxUsers : FoxResource
    {
        public FoxUserDto CurrentUser { get; private set; }

        public FoxUsers(FoxApi api) : base(api)
        {
        }
        
        public override async void OnPostLogin()
        {
            var profileResp = await API.Get<FoxProfileDto>("user/profile");
            CurrentUser = profileResp?.user;
            
            base.OnPostLogin();
        }

        public override void Dispose()
        {
            CurrentUser = null;
        }
    }
}