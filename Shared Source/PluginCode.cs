using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using CustomPluginObjects;
using RM;

// The bulk of your plugin's code belongs in this file.
namespace SvnPlugin
{
    class PluginCode
    {
        private string bat = @"c:\ps1\SVN\querysvn.bat";
        private SvnInfo svn;
        private string path;
        private int width = 20;
     
        // 'Update', 'Update2', and 'GetString' all return data back to Rainmeter, depending on
        // if the Rainmeter measure wants a numeric value or a string/text data.
        //
        // The 'Instance' member contains all of the data necessary to read the INI file
        // passed to your plugin when this instance was first initialized.  Remember: your plugin
        // may be initialized multiple times.  For example, a plugin that reads the free space
        // of a hard drive may be called multiple times -- once for each installed hard drive.

        public UInt32 Update(Rainmeter.Settings.InstanceSettings Instance)
        {
            return 7;
        }

        public double Update2(Rainmeter.Settings.InstanceSettings Instance)
        {
            return 7.0;
        }


        public string GetString(Rainmeter.Settings.InstanceSettings Instance)
        {
            var counter = 0;
            var updateCounter = 1;
            var retVal = "Error";

            if (!String.IsNullOrEmpty(Instance.GetVariable("counter")))
                counter = Convert.ToInt32(Instance.GetVariable("counter"));

            if (!String.IsNullOrEmpty(Instance.INI_Value("UpdateDivider")))
                updateCounter = Convert.ToInt32(Instance.INI_Value("UpdateDivider"));

            if (!String.IsNullOrEmpty(Instance.GetVariable("retVal")))
                retVal = Instance.GetVariable("retVal");
            
            if (!String.IsNullOrEmpty(Instance.INI_Value("Width")))
                width = Convert.ToInt16(Instance.INI_Value("Width"));

            if (counter % updateCounter == 0)
            {
                
                try
                {
                    GetSvnInfo(Instance);
                    retVal = svn.Alias + new string(' ', width - svn.Alias.Length - svn.Updates.Length) + svn.Updates;
                    Instance.SetVariable("retVal", retVal);
                }
                catch (Exception e)
                {
                    Debug.Write("Exception", e.ToString());
                }
                return retVal;
            }
            else
            {
                Instance.SetVariable("counter", counter++);
                return retVal;
            }
        }


        // 'ExecuteBang' is a way of Rainmeter telling your plugin to do something *right now*.
        // What it wants to do can be defined by the 'Command' parameter.
        public void ExecuteBang(Rainmeter.Settings.InstanceSettings Instance, string Command)
        {
            return;
        }

        public void WriteBatch()
        {
            if (File.Exists(bat)) return;
            StreamWriter w = new StreamWriter(bat, false);
            w.WriteLine("@ECHO OFF");
            w.WriteLine("svn info --xml %1");
            w.Close();
            w.Dispose();
        }

        public void GetSvnInfo(Rainmeter.Settings.InstanceSettings Instance)
        {
            path = Instance.INI_Value("LocalRepoPath");
            WriteBatch();
            
            var alias = Instance.INI_Value("RepoAlias");
            
            var logLimit = Convert.ToInt16(Instance.INI_Value("LogLimit"));

            var p = new Process
                        {
                            StartInfo =
                                {
                                    CreateNoWindow = true,
                                    UseShellExecute = false,
                                    RedirectStandardOutput = true,
                                    FileName = bat,
                                    Arguments = path,
                                    WindowStyle = ProcessWindowStyle.Hidden
                                }
                            };
            p.StartInfo.UseShellExecute = false;
            p.Start();
            string output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();

            svn = new SvnInfo(alias, output, logLimit);
        }
    }
}
