using System;
using System.Collections.Generic;

namespace CharacterControls.Movements.Modules
{
    public class ModuleRequestManager
    {
        private readonly List<IDisposable> _requests = new List<IDisposable>();
        public IDisposable GetRequest()
        {
            var request = new Request(this);
            _requests.Add(request);
            return request;
        }

        private void ReturnRequest(IDisposable request)
        {
            _requests.Remove(request);
        }

        public bool HasRequest()
        {
            return _requests.Count > 0;
        }

        private class Request : IDisposable
        {
            private readonly ModuleRequestManager _requestManager;
            public Request(ModuleRequestManager requestManager)
            {
                _requestManager = requestManager;
            }

            public void Dispose()
            {
                _requestManager.ReturnRequest(this);
            }
        }
    }
}
