﻿using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Coordinator.SSSB
{
    public class ServiceMessageEventArgs : EventArgs, IDisposable
    {
        private readonly ISSSBService _service;
        private readonly SSSBMessage _message;
        private readonly CancellationToken _token;
        private int _taskID;
        private readonly Task<HandleMessageResult> _completion;
        private readonly TaskCompletionSource<HandleMessageResult> _tcs;
        private readonly IServiceScope _serviceScope;
        private readonly IServiceProvider _services;

        public ServiceMessageEventArgs(SSSBMessage message, ISSSBService svc, CancellationToken cancellation, IServiceScope serviceScope)
        {
            _message = message;
            _service = svc;
            _token = cancellation;
            _taskID = -1;
            _serviceScope = serviceScope;
            _tcs = new TaskCompletionSource<HandleMessageResult>();
            _completion = _tcs.Task;
            _services = _serviceScope.ServiceProvider;
        }

        public TaskCompletionSource<HandleMessageResult> TaskCompletionSource
        {
            get
            {
                return _tcs;
            }
        }

        public SSSBMessage Message
        {
            get { return _message; }
        }

        public ISSSBService SSSBService
        {
            get
            {
                return this._service;
            }
        }

        public int TaskID
        {
            get
            {
                return _taskID;
            }
            set
            {
                _taskID = value;
            }
        }

        public CancellationToken Token
        {
            get
            {
                return _token;
            }
        }

        public Task<HandleMessageResult> Completion
        {
            get { return _completion; }
        }

        public IServiceProvider Services
        {
            get
            {
               return _services;
            }
        }

        public void Dispose()
        {
            _serviceScope.Dispose();
        }
    }
}
