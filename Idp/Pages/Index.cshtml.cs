// Copyright (c) Duende Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Open.IdentityServer;
using Open.IdentityServer.Licensing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Reflection;

namespace Idp.Pages.Home;

[AllowAnonymous]
public class Index : PageModel
{
    private readonly LicenseInformation _license;

    public Index(LicenseInformation license)
    {
        _license = license;
    }

    public void OnGet()
    {
        if (_license.IsConfigured)
        {
            // licensed behavior
        }
    }
}
