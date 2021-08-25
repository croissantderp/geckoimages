using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.ComponentModel;
using System.Threading;
using System.Management.Automation;
using Google.Apis.Drive.v3;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Drive.v3.Data;
using System.Threading.Tasks;

namespace geckoimagesBackend
{
    class Program
    {
        static readonly string[] Scopes = { DriveService.Scope.DriveReadonly };

        static void Main(string[] args)
        {
            Check check = new Check();
            check.checkDrive().GetAwaiter().GetResult();
        }
    }
}
