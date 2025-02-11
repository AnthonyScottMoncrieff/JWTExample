﻿using JWTExample.Domain.Attributes;
using JWTExample.Domain.Helpers.Interfaces;
using JWTExample.Domain.Services.Interfaces;
using JWTExample.Models.Auth;
using JWTExample.Models.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace JWTExample.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;
        private readonly IAccountControllerHelpers _accountControllerHelpers;

        private Account _account => (Account)HttpContext.Items["Account"];

        public AccountController(IAccountService accountService, IAccountControllerHelpers accountControllerHelpers)
        {
            _accountService = accountService;
            _accountControllerHelpers = accountControllerHelpers;
        }

        [HttpPost("authenticate")]
        public async Task<IActionResult> Authenticate(AuthenticateRequest model)
        {
            var response = await _accountService.AuthenticateAsync(model, _accountControllerHelpers.GetIpAddress(Response, HttpContext));
            _accountControllerHelpers.SetTokenCookie(response.RefreshToken, Response);
            return Ok(response);
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            var ipAddress = _accountControllerHelpers.GetIpAddress(Response, HttpContext);
            var response = await _accountService.RefreshTokenAsync(refreshToken, ipAddress);
            _accountControllerHelpers.SetTokenCookie(response.RefreshToken, Response);
            return Ok(response);
        }

        [BespokeAuthorize]
        [HttpPost("revoke-token")]
        public async Task<IActionResult> RevokeToken(RevokeTokenRequest model)
        {
            // accept token from request body or cookie
            var token = model.Token ?? Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(token))
                return BadRequest(new { message = "Token is required" });

            // users can revoke their own tokens and admins can revoke any tokens
            if (!_account.OwnsToken(token) && _account.Role != Role.Admin)
                return Unauthorized(new { message = "Unauthorized" });

            await _accountService.RevokeTokenAsync(token, _accountControllerHelpers.GetIpAddress(Response, HttpContext));
            return Ok(new { message = "Token revoked" });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest model)
        {
            await _accountService.RegisterAsync(model);
            return Ok(new { message = "Registration successful, please check your email for verification instructions" });
        }

        [HttpPost("validate-reset-token")]
        public async Task<IActionResult> ValidateResetToken(ValidateResetTokenRequest model)
        {
            await _accountService.ValidateResetTokenAsync(model);
            return Ok(new { message = "Token is valid" });
        }

        [BespokeAuthorize(Role.Admin)]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var accounts = await _accountService.GetAllAsync();
            return Ok(accounts);
        }

        [BespokeAuthorize]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            // users can get their own account and admins can get any account
            if (id != _account.Id && _account.Role != Role.Admin)
                return Unauthorized(new { message = "Unauthorized" });

            var account = await _accountService.GetByIdAsync(id);
            return Ok(account);
        }

        [BespokeAuthorize(Role.Admin)]
        [HttpPost]
        public async Task<IActionResult> Create(CreateRequest model)
        {
            var account = await _accountService.CreateAsync(model);
            return Ok(account);
        }

        [BespokeAuthorize]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, UpdateRequest model)
        {
            // users can update their own account and admins can update any account
            if (id != _account.Id && _account.Role != Role.Admin)
                return Unauthorized(new { message = "Unauthorized" });

            // only admins can update role
            if (_account.Role != Role.Admin)
                model.Role = null;

            var account = await _accountService.UpdateAsync(id, model);
            return Ok(account);
        }

        [BespokeAuthorize]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            // users can delete their own account and admins can delete any account
            if (id != _account.Id && _account.Role != Role.Admin)
                return Unauthorized(new { message = "Unauthorized" });

            await _accountService.DeleteAsync(id);
            return Ok(new { message = "Account deleted successfully" });
        }
    }
}