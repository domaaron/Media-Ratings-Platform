using Media_Ratings_Platform.DTOs;
using MediaRatings.Api.Utils;
using MediaRatings.Domain.services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MediaRatings.Api.controllers
{
    public class AuthController
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        public async Task RegisterAsync(HttpListenerContext context)
        {
            var data = await JsonHelper.ReadBodyAsync<RegisterDto>(context.Request);
            if (data == null)
            {
                await HttpHelper.WriteTextAsync(context.Response, 400, "Invalid JSON");
                return;
            }

            try
            {
                await _authService.RegisterUserAsync(data.Username, data.Password);
                await HttpHelper.WriteTextAsync(context.Response, 201, "User created");
            }
            catch (InvalidOperationException ex)
            {
                await HttpHelper.WriteTextAsync(context.Response, 409, ex.Message);
            }
        }

        public async Task LoginAsync(HttpListenerContext context)
        {
            var data = await JsonHelper.ReadBodyAsync<LoginDto>(context.Request);
            if (data == null)
            {
                await HttpHelper.WriteTextAsync(context.Response, 400, "Invalid JSON");
                return;
            }

            var token = await _authService.LoginAsync(data.Username, data.Password);
            if (token == null)
            {
                await HttpHelper.WriteTextAsync(context.Response, 401, "Unauthorized");
            }
            else
            {
                await HttpHelper.WriteJsonAsync(context.Response, 200, new { token });
            }
        }
    }
}
