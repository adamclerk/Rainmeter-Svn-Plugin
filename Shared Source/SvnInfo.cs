using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;

namespace CustomPluginObjects
{
    public class SvnInfo
    {
        public string Url;
        public string RepoRoot;
        public string Uuid;
        public string Path;
        public string Alias;
        public string Updates;
        public string LatestRevision;

        private int _logLimit;
        private SvnRemoteRepo remote;

        public SvnInfo(string alias, string xml, int logLimit)
        {
            try
            {
                Alias = alias;
                Updates = "0";
                _logLimit = logLimit;
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xml);
                Path = doc["info"]["entry"].Attributes["path"].Value;
                RepoRoot = doc["info"]["entry"]["repository"]["root"].InnerText;
                Url = doc["info"]["entry"]["url"].InnerText;
                Uuid = doc["info"]["entry"]["repository"]["uuid"].InnerText;
                LatestRevision = doc["info"]["entry"]["commit"].Attributes["revision"].Value;

                remote = new SvnRemoteRepo(Url, _logLimit);
                Updates = remote.GetUpdates(LatestRevision);
            }
            catch (Exception e)
            {
                Debug.Write("Exception", e.ToString());
            }            
        }
    }

    class SvnRemoteRepo
    {
        public string Url;
        public IList<string> Revisions;
        private string bat = @"c:\ps1\Svn\svnlog.bat";
        private int loglimit;

        public SvnRemoteRepo(string url, int logLimit)
        {
            Url = url;
            loglimit = logLimit;
            Revisions = new List<string>();
            ExecuteBatch(url);
        }

        public string GetUpdates(string LatestRevision)
        {
            //return "0";
            int updates = 0;
            foreach(string rev in Revisions)
            {
                if (rev == LatestRevision)
                    return updates.ToString();
                updates++;       
            }
            return updates.ToString();
        }

        private void WriteBatch(string path)
        {
            if (File.Exists(bat)) return;
            StreamWriter w = new StreamWriter(bat, false);
            w.WriteLine("@ECHO OFF");
            w.WriteLine("svn log --xml --with-no-revprops --limit " + loglimit + " %1");
            w.Close();
            w.Dispose();
        }

        private void ExecuteBatch(string path)
        {
            WriteBatch(path);

            var p = new Process
            {
                StartInfo =
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    Arguments = path,
                    FileName = bat,
                    WindowStyle = ProcessWindowStyle.Hidden
                }
            };
            p.StartInfo.UseShellExecute = false;
            p.Start();
            string output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(output);

            foreach(XmlNode logEntry in doc["log"].ChildNodes)
            {
                //WriteDebug("logentry", logEntry.Attributes[0].Value);
                Revisions.Add(logEntry.Attributes[0].Value);
            }
        }
    }
}
