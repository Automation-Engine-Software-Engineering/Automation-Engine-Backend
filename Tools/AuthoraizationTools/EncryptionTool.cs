using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;

namespace Tools.AuthoraizationTools
{
    public class EncryptionTool
    {
        private readonly IDataProtector _protector;

        public EncryptionTool(IDataProtectionProvider provider)
        {
            _protector = provider.CreateProtector("MyCookieProtection");
        }

        public string? EncryptCookie(string? value)
        {
            if (value == null)
                return null;
            return _protector.Protect(value);
        }

        public string? DecryptCookie(string? encryptedValue)
        {
            if (encryptedValue == null)
                return null;
            return _protector.Unprotect(encryptedValue);
        }
    }
}
