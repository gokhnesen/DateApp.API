﻿using AutoMapper;
using DateApp.Data.DTOs;
using DateApp.Data.Extensions;
using DateApp.Data.Interfaces;
using DateApp.Entity.Entities;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DateApp.API.SignalR
{
    public class MessageHub:Hub
    {
        private readonly IMessageRepository _messageRepository;
        private readonly IMapper _mapper;
        private readonly IUserRepository _userRepository;
        public MessageHub(IMessageRepository messageRepository,IMapper mapper,IUserRepository userRepository)
        {
            _mapper = mapper;
            _messageRepository = messageRepository;
            _userRepository = userRepository;
        }

        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var otherUser = httpContext.Request.Query["user"].ToString();
            var groupName = GetGroupName(Context.User.GetUsername(), otherUser);
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            await AddToGroup(Context,groupName);
            var messages = await _messageRepository.GetMessageThread(Context.User.GetUsername(), otherUser);

            await Clients.Group(groupName).SendAsync("ReceiveMessageThread", messages);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await RemoveFromMessageGroup(Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }
        public async Task SendMessage(CreateMessageDto createMessageDto)
        {
            var username = Context.User.GetUsername();
            if (username == createMessageDto.RecipientUsername.ToLower())
                throw new HubException("You cannot send messages to yoursel");

            var sender = await _userRepository.GetUserByUsernameAsync(username);
            var recipient = await _userRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername);
            if (recipient == null) throw new HubException("Not Found User");

            var message = new Message
            {
                Sender = sender,
                Recipient = recipient,
                SenderUsername = sender.UserName,
                RecipientUsername = recipient.UserName,
                Content = createMessageDto.Content
            };
            var groupName = GetGroupName(sender.UserName, recipient.UserName);
            var group = await _messageRepository.GetMessageGroup(groupName);
            if(group.Connections.Any(x=>x.Username == recipient.UserName))
            {
                message.DateRead = DateTime.UtcNow;
            }

            _messageRepository.AddMessage(message);
            
            if (await _messageRepository.SaveAllAsync())
            {
                await Clients.Group(groupName).SendAsync("NewMessage", _mapper.Map<MessageDto>(message));
            }
        }

        private async Task<bool> AddToGroup(HubCallerContext context,string groupName)
        {
            var group = await _messageRepository.GetMessageGroup(groupName);
            var connection = new Connection(Context.ConnectionId, Context.User.GetUsername());
            if (group == null)
            {
                group = new Group(groupName);
                _messageRepository.AddGroup(group);
            }
            group.Connections.Add(connection);
            return await _messageRepository.SaveAllAsync();
        }

        private async Task RemoveFromMessageGroup(string connectionId)
        {
            var connection = await _messageRepository.GetConnection(connectionId);
            _messageRepository.RemoveConnection(connection);
            await _messageRepository.SaveAllAsync();
        }

        private string GetGroupName(string caller,string other)
        {
            var stringCompare = string.CompareOrdinal(caller, other) < 0;
            return stringCompare ? $"{caller}--{other}" : $"{other}--{caller}";
        }
    }
}
