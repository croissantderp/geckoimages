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
using System.Text.Json;
using System.Timers;

namespace geckoimagesBackend
{
    public class gecko
    {
        public string name { get; set; }
        public string author { get; set; }
        public DateTime time { get; set; }
        public string driveLink { get; set; }

    }

    public class Check
    {
        public async Task setTimer()
        {
            System.Timers.Timer t = new System.Timers.Timer(10 * 60 * 1000);
            t.Elapsed += async (sender, e) => await checkDrive();
            t.AutoReset = true;
            t.Start();

            await checkDrive();

            await Task.Delay(-1);
        }

        public async Task checkDrive()
        {
            Console.WriteLine("Checking drive");

            List<gecko> geckos = new List<gecko>();

            if (System.IO.File.Exists(@"C:/xampp/htdocs/db.json"))
            {
                //gets list of geckos already in database
                StreamReader dbRead = new StreamReader(@"C:/xampp/htdocs/db.json");
                geckos = JsonSerializer.Deserialize<List<gecko>>(dbRead.ReadToEnd());
                dbRead.Close();
            }

            //signs into drive
            DriveService driveService = DriveUtils.AuthenticateServiceAccount(
                "geckoimagerworker@geckoimagesworker.iam.gserviceaccount.com",
                "geckoimagesworker-b3ff87875739.json");

            //requests all files in batches of 100
            var listRequest = driveService.Files.List();
            listRequest.PageSize = 100;
            listRequest.OrderBy = "name desc";
            listRequest.Fields = @"nextPageToken, files(*)";

            int count = 0;
            int updateCount = 0;

            bool highestFound = false;
            int highestGecko = 0;

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
                        if (!geckos.Select(a => a.name).Contains(a.Name) && new Regex(@"^(?:b|)\d+_.+").Match(a.Name).Success)
                        {
                            count++;

                            if (!highestFound && new Regex(@"^\d+_.+").Match(a.Name).Success)
                            {
                                highestGecko = int.Parse(a.Name.Remove(3));
                                highestFound = true;
                            }

                            //adds gecko to database
                            geckos.Add(new gecko
                            { 
                                name = a.Name,
                                author = a.Description != null && a.Description != "" ? a.Description : a.Owners.First().DisplayName,
                                time = DateTime.Parse(a.CreatedTimeRaw),
                                driveLink = a.WebViewLink
                            });

                            string name = a.Name.Remove(3);
                            if (name.Contains("b")) name = a.Name.Remove(4);
                            
                            
                            //downloads file
                            using var fileStream = new FileStream(
                                $"C:/xampp/htdocs/{name}.{a.Name.Split(".").Last()}",
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
                            file.Name = highestGecko + "_" + string.Join(" - ", splitName).Replace(" ", "_") + "." + extension;
                            highestGecko++;

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
                StreamWriter dbWrite = new StreamWriter(@"C:/xampp/htdocs/db.json");
                await dbWrite.WriteAsync(JsonSerializer.Serialize(geckos, new JsonSerializerOptions{ WriteIndented = true }));
                dbWrite.Close();

                //await deploy();
            }

            Console.WriteLine($"Done, added {count} files, updated {updateCount} files in submissions folder");
        }

        /*
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
        */
    }
}
