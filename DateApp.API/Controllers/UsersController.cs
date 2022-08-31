﻿using AutoMapper;
using DateApp.Data.Abstract;
using DateApp.Data.DTOs;
using DateApp.Entity.DataContext;
using DateApp.Entity.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DateApp.API.Controllers
{
    [Authorize]

    public class UsersController : BaseApiController
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        public UsersController(IUserRepository userRepository,IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task< ActionResult<IEnumerable<MemberDto>>> GetUsers()
        {
            var users = await _userRepository.GetMembersAsync();
            return Ok(users);
        }
        //api/users/3
       
        [HttpGet("{username}")]
        public async Task<ActionResult<MemberDto>> GetUser(string username)
        {
            return await _userRepository.GetMemberAsync(username);
          
        }

        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
        {
            var username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _userRepository.GetUserByUsernameAsync(username);
            
            _mapper.Map(memberUpdateDto,user);
            _userRepository.Update(user);
            if (await _userRepository.SaveAllAsync()) return NoContent();
            return BadRequest("Güncelleme başarısız");
        }
    }
}
