﻿using Auth.Application.EventData;
using Auth.Application.MediatR;
using System.ComponentModel.DataAnnotations;
using Utilities.Responses;
using Utilities.Validations;

namespace Auth.Application.Commands
{
    /// <summary>
    /// Command for user login.
    /// </summary>
    public class LoginCommand : ICommand
    {
        /// <summary>
        /// Email of the user to be authenticated.
        /// </summary>
        [EmailAddress]
        public required string Email { get; set; }
        /// <summary>
        /// Occurs when the user is Created User and return the user token model.
        /// </summary>
        public event EventHandler<string>? CreatedTokenModel;

        internal virtual void OnAuthenticated(string refreshTokenArgs)
        {
            CreatedTokenModel?.Invoke(this, refreshTokenArgs);
        }
        /// <summary>
        /// Password of the user to be authenticated.
        /// </summary>
        public required string Password { get; set; }

        /// <summary>
        /// Validates the email address for the user.
        /// </summary>
        /// <returns>An instance of <see cref="KOActionResult"/> indicating the result of the validation.</returns>
        public KOActionResult Validate()
        {
           return new KOActionResult();
        }
    }
    /// <summary>
    /// Handles a <see cref="LoginCommand"/>.
    /// </summary>
    public class LoginHandler : ICommandHandler<LoginCommand>
    {
        /// <summary>
        /// Handles a <see cref="LoginCommand"/>.
        /// </summary>
        /// <param name="command">The command to handle.</param>
        /// <param name="service">The service wrapper.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="KOActionResult"/> representing the result of the operation.</returns>
        public async Task<KOActionResult> HandleAsync(LoginCommand command, IServiceWrapper service, CancellationToken cancellationToken = default)
        {
            var result = new KOActionResult();
            var user= await service.UserRepo.FindOneByPredicate(x=>x.Email==command.Email.ToLower().Trim());

            if (user is null)
            {
                result.AddError("Invalid login details.");
                return result;
            }

            if (!service.AuthService.VerifyPassword(command.Password, user))
            {
                result.AddError("Invalid login details.");
                return result;
            }

            var tokenModel = await service.AuthService.TokenManager(user);
            command.OnAuthenticated(tokenModel.RefreshToken);
            result.data=tokenModel.AccessToken;
            return result;



        }
    }
}
