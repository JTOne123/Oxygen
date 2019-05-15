﻿using MediatR;
using Oxygen.Common.Logger;
using Oxygen.ISerializeService;
using Oxygen.IServerProxyFactory;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Oxygen.MediatRProxyClientBuilder
{
    /// <summary>
    /// 本地代理消息分发处理类
    /// </summary>
    public class LocalProxyGenerator : ILocalProxyGenerator
    {
        private readonly IMediator _mediator;
        private static readonly Assembly MediatRAssembly = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(x => x.FullName.Contains("LocalClient.g"));
        private static readonly ConcurrentDictionary<string, Type> InstanceDictionary = new ConcurrentDictionary<string, Type>();
        private readonly IOxygenLogger _logger;
        private readonly ISerialize _serialize;
        public LocalProxyGenerator(IMediator mediator, IOxygenLogger logger, ISerialize serialize)
        {
            _mediator = mediator;
            _logger = logger;
            _serialize = serialize;
        }

        /// <summary>
        /// 消息分发处理
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task<byte[]> Invoke(byte[] message)
        {
            if (message == null || !message.Any())
            {
                _logger.LogError($"订阅者消息分发不能为空消息");
                return default(byte[]);
            }
            else
            {
                var messageBody = _serialize.Deserializes<RpcGlobalMessageBase<object>>(message);
                if (!InstanceDictionary.TryGetValue(messageBody.Path, out var messageType))
                {
                    messageType = MediatRAssembly.GetType($"Oxygen.MediatRProxyClientBuilder.ProxyInstance.{messageBody.Path}");
                    if (messageType != null)
                    {
                        InstanceDictionary.TryAdd(messageBody.Path, messageType);
                    }
                }
                if (messageType != null)
                {
                    messageBody.Message = await Publish(_serialize.Deserializes(messageType, _serialize.Serializes(messageBody.Message)));
                    return _serialize.Serializes(messageBody);
                }
                else
                {
                    _logger.LogError($"未找到订阅者实例：{messageBody.Path}");
                }
                return default(byte[]);
            }
        }

        /// <summary>
        /// 发布消息
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private async Task<dynamic> Publish(dynamic message)
        {
            return await _mediator.Send(message);
        }
    }
}
