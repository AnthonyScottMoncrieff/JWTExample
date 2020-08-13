using JWTExample.Data;
using JWTExample.Domain.Helpers.Interfaces;
using JWTExample.Domain.Mappers.Interfaces;
using JWTExample.Domain.Services.Interfaces;
using JWTExample.Models.Auth;
using JWTExample.Models.Entities;
using JWTExample.Models.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BC = BCrypt.Net.BCrypt;

namespace JWTExample.Domain.Services
{
    public class AccountService : IAccountService
    {
        private readonly DataContext _context;
        private readonly IAppSettings _appSettings;
        private readonly IMapper<Account, AccountResponse> _accountResponseMapper;
        private readonly IMapper<CreateRequest, Account> _createRequestMapper;
        private readonly IMapper<Account, AuthenticateResponse> _authMapper;
        private readonly IMapper<RegisterRequest, Account> _registerRequestMapper;
        private readonly IAccountUpdater _accountUpdater;
        private readonly IAccountServiceHelpers _accountServiceHelpers;

        public AccountService(
            DataContext context,
            IAppSettings appSettings,
            IMapper<Account, AccountResponse> accountResponseMapper,
            IMapper<CreateRequest, Account> createRequestMapper,
            IMapper<Account, AuthenticateResponse> authMapper,
            IMapper<RegisterRequest, Account> registerRequestMapper,
            IAccountUpdater accountUpdater,
            IAccountServiceHelpers accountServiceHelpers)
        {
            _context = context;
            _appSettings = appSettings;
            _accountResponseMapper = accountResponseMapper;
            _createRequestMapper = createRequestMapper;
            _authMapper = authMapper;
            _registerRequestMapper = registerRequestMapper;
            _accountUpdater = accountUpdater;
            _accountServiceHelpers = accountServiceHelpers;
        }

        public async Task<AuthenticateResponse> AuthenticateAsync(AuthenticateRequest model, string ipAddress)
        {
            var account = await _context.Accounts.SingleOrDefaultAsync(x => x.Email == model.Email);

            if (account == null || !BC.Verify(model.Password, account.PasswordHash))
                throw new Exception("Email or password is incorrect");

            // authentication successful so generate jwt and refresh tokens
            var jwtToken = _accountServiceHelpers.GenerateJwtToken(account, _appSettings);
            var refreshToken = _accountServiceHelpers.GenerateRefreshToken(ipAddress);

            // save refresh token
            account.RefreshTokens.Add(refreshToken);
            _context.Update(account);
            await _context.SaveChangesAsync();

            var response = _authMapper.Map(account);
            response.JwtToken = jwtToken;
            response.RefreshToken = refreshToken.Token;
            return response;
        }

        public async Task<AuthenticateResponse> RefreshTokenAsync(string token, string ipAddress)
        {
            var (refreshToken, account) = await _accountServiceHelpers.GetRefreshTokenAsync(token, _context);

            // replace old refresh token with a new one and save
            var newRefreshToken = _accountServiceHelpers.GenerateRefreshToken(ipAddress);
            refreshToken.Revoked = DateTime.UtcNow;
            refreshToken.RevokedByIp = ipAddress;
            refreshToken.ReplacedByToken = newRefreshToken.Token;
            account.RefreshTokens.Add(newRefreshToken);
            _context.Update(account);
            await _context.SaveChangesAsync();

            // generate new jwt
            var jwtToken = _accountServiceHelpers.GenerateJwtToken(account, _appSettings);

            var response = _authMapper.Map(account);
            response.JwtToken = jwtToken;
            response.RefreshToken = newRefreshToken.Token;
            return response;
        }

        public async Task RevokeTokenAsync(string token, string ipAddress)
        {
            var (refreshToken, account) = await _accountServiceHelpers.GetRefreshTokenAsync(token, _context);

            // revoke token and save
            refreshToken.Revoked = DateTime.UtcNow;
            refreshToken.RevokedByIp = ipAddress;
            _context.Update(account);
            await _context.SaveChangesAsync();
        }

        public async Task RegisterAsync(RegisterRequest model)
        {
            // validate
            var accountExists = await _context.Accounts.AnyAsync(x => x.Email == model.Email);
            if (accountExists)
            {
                // send already registered error in email to prevent account enumeration
                throw new Exception("User already registered");
            }

            // map model to new account object
            var account = _registerRequestMapper.Map(model);

            // first registered account is an admin
            var isFirstAccount = (await _context.Accounts.CountAsync()) == 0;
            account.Role = isFirstAccount ? Role.Admin : Role.User;
            account.Created = DateTime.UtcNow;

            // hash password
            account.PasswordHash = BC.HashPassword(model.Password);

            // save account
            await _context.Accounts.AddAsync(account);
            await _context.SaveChangesAsync();
        }

        public async Task ValidateResetTokenAsync(ValidateResetTokenRequest model)
        {
            var account = await _context.Accounts.SingleOrDefaultAsync(x =>
                x.ResetToken == model.Token &&
                x.ResetTokenExpires > DateTime.UtcNow);

            if (account == null)
                throw new Exception("Invalid token");
        }

        public async Task<IEnumerable<AccountResponse>> GetAllAsync()
        {
            var accounts = await _context.Accounts.ToListAsync();
            return accounts.Select(x => _accountResponseMapper.Map(x));
        }

        public async Task<AccountResponse> GetByIdAsync(int id)
        {
            var account = await _accountServiceHelpers.GetAccountAsync(id, _context);
            return _accountResponseMapper.Map(account);
        }

        public async Task<AccountResponse> CreateAsync(CreateRequest model)
        {
            // validate
            var matchingAccounts = await _context.Accounts.AnyAsync(x => x.Email == model.Email);
            if (matchingAccounts)
                throw new Exception($"Email '{model.Email}' is already registered");

            // map model to new account object
            var account = _createRequestMapper.Map(model);
            account.Created = DateTime.UtcNow;

            // hash password
            account.PasswordHash = BC.HashPassword(model.Password);

            // save account
            await _context.Accounts.AddAsync(account);
            await _context.SaveChangesAsync();

            return _accountResponseMapper.Map(account);
        }

        public async Task<AccountResponse> UpdateAsync(int id, UpdateRequest model)
        {
            var account = await _accountServiceHelpers.GetAccountAsync(id, _context);

            // validate
            var matchingAccounts = await _context.Accounts.AnyAsync(x => x.Email == model.Email);
            if (account.Email != model.Email && matchingAccounts)
                throw new Exception($"Email '{model.Email}' is already taken");

            // hash password if it was entered
            if (!string.IsNullOrEmpty(model.Password))
                account.PasswordHash = BC.HashPassword(model.Password);

            // copy model to account and save
            _accountUpdater.Update(model, account);
            account.Updated = DateTime.UtcNow;
            _context.Accounts.Update(account);
            await _context.SaveChangesAsync();

            return _accountResponseMapper.Map(account);
        }

        public async Task DeleteAsync(int id)
        {
            var account = await _accountServiceHelpers.GetAccountAsync(id, _context);
            _context.Accounts.Remove(account);
            _context.SaveChanges();
        }
    }
}