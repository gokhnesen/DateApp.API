﻿using DateApp.Entity.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DateApp.Data.Interfaces
{
   public interface ITokenService
    {
        Task<string> CreateToken(AppUser user);
    }
}
