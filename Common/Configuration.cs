﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorDemoCRUD.Common
{
    public class Configuration
    {
        public string ClientId { get; set; }
        public string Authority { get; set; }
        public bool ValidateAuthority { get; set; }
        public string DefaultAccessTokenScopes { get; set; }
    }
}
