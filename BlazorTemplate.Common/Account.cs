using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorTemplate.Common
{
    public class AccountEmail
    {
        public string Email { get; set; }
    }

    public class IdentityManageUserResponse
    {
        public string Email { get; set; }
        public bool IsEmailConfirmed { get; set; }
    }

    public class IdentityManage2faResponse
    {
        public string SharedKey { get; set; }
        public int RecoveryCodesLeft { get; set; }
        public string[] RecoveryCodes { get; set; }
        public bool IsTwoFactorEnabled { get; set; }
        public bool IsMachineRemembered { get; set; }
    }

    public class LoginResponse
    {
        public string Type { get; set; }
        public string Title { get; set; }
        public string Detail { get; set; }
        public int Status { get; set; }
    }
}
