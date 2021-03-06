﻿using System;
using Topos.Config;

namespace Topos.Tests.Contracts
{
    public interface IBrokerFactory : IDisposable
    {
        ToposProducerConfigurer ConfigureProducer();
        ToposConsumerConfigurer ConfigureConsumer(string groupName);
        string GetNewTopic();
    }
}