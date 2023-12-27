using System;

namespace Furality.SDK.External.Api
{
    public abstract class FoxResource : IDisposable
    {
        protected readonly FoxApi API;
        public bool HasFinishedLogin { get; private set; }
        public Action OnLoggedIn;

        protected FoxResource(FoxApi api)
        {
            API = api;
            OnLoggedIn = () => HasFinishedLogin = true;
        }

        // Gives us an opportunity to do some async requests for basic data to be displayed in the GUI
        // Called after login
        public virtual void OnPostLogin()
        {
            OnLoggedIn();
        }

        public abstract void Dispose();
    }
}