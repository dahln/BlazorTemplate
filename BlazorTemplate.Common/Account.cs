using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorTemplate.Common
{
    public class IdentityManageInfoUpdate
    {
        public string NewEmail { get; set; }
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }
    
    public class IdentityManageUserResponse
    {
        public string Email { get; set; }
        public bool IsEmailConfirmed { get; set; }
    }


    public class IdentityForgotPassword
    {
        public string Email { get; set; }
    }

    public class IdentityResetPassword
    {
        public string Email { get; set; }
        public string ResetCode { get; set; }
        public string NewPassword { get; set; }
    }

    public class IdentityLogin 
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class IdentityRegister
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string TwoFactorCode { get; set; }
        public string TwoFactorRecoveryCode { get; set; }
    }

    public class IdentityResendEmailConfirmation
    {
        public string Email { get; set; }
    }

    public class AccountEmail
    {
        public string Email { get; set; }
    }
}
