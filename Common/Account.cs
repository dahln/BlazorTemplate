﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorDemoCRUD.Common
{
    public class ChangeName
    {
        public string NewName { get; set; }
    }
    
    public class ChangeEmail
    {
        public string NewEmail { get; set; }
    }

    public class ChangePassword
    {
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
    }
}
