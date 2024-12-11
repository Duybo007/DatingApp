using System.Security.Claims;
using API.DTOs;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;
[Authorize]
public class UsersController(IUserRepository userRepository, IMapper mapper) : BaseApiController
{
    [HttpGet]   //  /api/users
    public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers()
    {
        var users = await userRepository.GetMembersAsync();

        return Ok(users);
    }


    [HttpGet("{username}")]    //  /api/users/bob
    public async Task<ActionResult<MemberDto>> GetUser(string username)
    {
        var user = await userRepository.GetMemberAsync(username);

        if (user == null) return NotFound();

        return user;
    }

    [HttpPut]
    public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
    {
        var username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;  // what we use in token service

        if (username == null) return BadRequest("No username found in token");

        var user = await userRepository.GetUserByNameAsync(username);

        if(user == null) return BadRequest("Cound not find use");

        mapper.Map(memberUpdateDto, user);

        if(await userRepository.SaveAllSync()) return NoContent();

        return BadRequest("Failed to update the user");
    }
}
