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
    public class Check
    {
        public async Task checkDrive()
        {
            Console.WriteLine("Checking drive");

            //gets list of geckos already in database
            StreamReader dbRead = new StreamReader(@"../../../../public/geckos/db.txt");
            List<string> geckos = Regex.Split(await dbRead.ReadToEndAsync(), @"\s(?<!\\)\,\s").ToList();
            dbRead.Close();

            //signs into drive
            DriveService driveService = DriveUtils.AuthenticateServiceAccount(
                "geckoimagerworker@geckoimagesworker.iam.gserviceaccount.com",
                "../../../../geckoimagesworker-b3ff87875739.json");

            //requests all files in batches of 100
            var listRequest = driveService.Files.List();
            listRequest.PageSize = 100;
            listRequest.OrderBy = "name desc";

            int count = 0;

            try
            {
                //iterates through the pages of 100
                while (true)
                {
                    FileList files2 = await listRequest.ExecuteAsync();
                    IList<Google.Apis.Drive.v3.Data.File> files = files2.Files;

                    foreach (Google.Apis.Drive.v3.Data.File a in files)
                    {
                        //if file is not in database and name matches naming conventions
                        if (!geckos.Contains(a.Name) && new Regex(@"^(?:b|)\d+_.+").Match(a.Name).Success)
                        {
                            count++;

                            //escapes comma character which is used for database then adds to database
                            geckos.Add(a.Name.Replace(",", @"\,"));

                            //downloads file
                            using var fileStream = new FileStream(
                                $"../../../../public/geckos/{a.Name}",
                                FileMode.Create,
                                FileAccess.Write);
                            await driveService.Files.Get(a.Id).DownloadAsync(fileStream);
                            fileStream.Close();
                        }
                    }

                    //scrolls to next page
                    listRequest.PageToken = files2.NextPageToken;

                    //if there is not a next page, quit
                    if (files2.NextPageToken == null)
                    {
                        break;
                    }

                }
            }
            catch
            {
                Console.WriteLine("drive check failed");
            }

            if (count != 0)
            {
                //writes updated list to database
                StreamWriter dbWrite = new StreamWriter(@"../../../../public/geckos/db.txt");
                await dbWrite.WriteAsync(string.Join(" , ", geckos));
                dbWrite.Close();

                await deploy();
            }

            Console.WriteLine($"Done, added {count} files");
        }

        private async Task deploy()
        {
            //ensure that the firebase cli is globally installed via npm (run `npm install -g firebase-tools`) or else this will error
            using (Process process = new Process())
            {
                //goes to main directory and runs thing
                process.StartInfo.WorkingDirectory = @"../../../../";
                process.StartInfo.FileName = @"../../../../firebase-tools-instant-win.exe";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardInput = true;
                //process.StartInfo.RedirectStandardOutput = true;
                process.Start();

                Thread.Sleep(10000);

                //runs deploy command
                await process.StandardInput.WriteLineAsync(@"firebase deploy --only hosting:geckos");
            };
        }

    }
}
