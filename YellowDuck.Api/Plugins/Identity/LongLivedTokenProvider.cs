﻿using YellowDuck.Api.DbModel.Entities;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System;
using Microsoft.Extensions.Logging;

namespace YellowDuck.Api.Plugins.Identity
{
    public class LongLivedTokenProvider : DataProtectorTokenProvider<AppUser>
    {
        public LongLivedTokenProvider(
            IDataProtectionProvider dataProtectionProvider,
            IOptions<LongLivedTokenProviderOptions> options,
            ILogger<DataProtectorTokenProvider<AppUser>> logger)
            : base(dataProtectionProvider, options, logger)
        {
        }
    }

    public class LongLivedTokenProviderOptions : DataProtectionTokenProviderOptions
    {
        public LongLivedTokenProviderOptions()
        {
            TokenLifespan = TimeSpan.FromDays(365);
        }
    }
}