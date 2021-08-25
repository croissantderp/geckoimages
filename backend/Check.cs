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
            int updateCount = 0;

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

                            string name = a.Name.Remove(3);
                            if (name.Contains("b")) name = a.Name.Remove(4);

                            //downloads file
                            using var fileStream = new FileStream(
                                $"../../../../public/geckos/{name}.{a.Name.Split(".").Last()}",
                                FileMode.Create,
                                FileAccess.Write);
                            await driveService.Files.Get(a.Id).DownloadAsync(fileStream);
                            fileStream.Close();
                        }
                        //else if file matches submission naming convention
                        else if (new Regex(@".+ - .+").Match(a.Name).Success)
                        {
                            updateCount++;

                            //new file to update
                            Google.Apis.Drive.v3.Data.File file = new Google.Apis.Drive.v3.Data.File();

                            //splits name and subsplits it
                            List<string> splitName = a.Name.Split(" - ").ToList();

                            List<string> nameSplit = splitName.Last().Split(".").ToList();

                            string extension = nameSplit.Last();

                            nameSplit.Remove(extension);

                            //updates description
                            file.Description = string.Join(".", nameSplit);

                            splitName.Remove(splitName.Last());

                            //updates name
                            file.Name = string.Join(" - ", splitName) + "." + extension;

                            //keeps same parents
                            file.Parents = a.Parents;

                            //updates file in drive
                            driveService.Files.Update(file, a.Id).Execute();
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
            catch (Exception ex)
            {
                Console.WriteLine("drive check failed, reason: " + ex.ToString());
            }

            if (count != 0)
            {
                //writes updated list to database
                StreamWriter dbWrite = new StreamWriter(@"../../../../public/geckos/db.txt");
                await dbWrite.WriteAsync(string.Join(" , ", geckos));
                dbWrite.Close();

                await deploy();
            }

            Console.WriteLine($"Done, added {count} files, updated {updateCount} files in submissions folder");
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
