﻿using System;
using Topos.Serialization;

namespace Topos.Consumer
{
    public class DefaultToposConsumer : IToposConsumer
    {
        readonly IConsumerImplementation _consumerImplementation;
        readonly IMessageSerializer _messageSerializer;

        bool _isStarted;

        bool _disposing;
        bool _disposed;

        public event Action Disposing;

        public DefaultToposConsumer(IMessageSerializer messageSerializer, IConsumerImplementation consumerImplementation)
        {
            _messageSerializer = messageSerializer;
            _consumerImplementation = consumerImplementation;
        }

        public void Start()
        {
            if (_isStarted) return;

            _consumerImplementation.Start();

            _isStarted = true;
        }

        public void Dispose()
        {
            if (_disposed) return;
            if (_disposing) return;

            _disposing = true;

            try
            {
                Disposing?.Invoke();
            }
            finally
            {
                _disposed = true;
            }
        }
    }
}