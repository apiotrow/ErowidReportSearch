using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Microsoft.CSharp;
using Newtonsoft.Json.Linq;

//foreach (KeyValuePair<string, string> entry in OpVals)
//{
//    Console.WriteLine(entry.Key + " " + entry.Value);
//}

namespace ErowidVoice {
    public class Program
    {
        public static void Main()
        {
            Dictionary<string, string> OpVals = new Dictionary<string, string>();
            List<string> IDs = new List<string>();

            scrapeOV(OpVals);

            string drugToSearch = "Mushrooms";
            string searchTerm = "voice";
            
            getIDs(drugToSearch, searchTerm, OpVals, IDs);

            Random r = new Random();
            string url = "https://erowid.org/experiences/exp.php?ID=" + IDs[r.Next(0, IDs.Count)];
            string report = getURLSource(url);
            report = report.Substring(report.IndexOf("<!-- Start Body -->"));
            report = report.Substring(report.IndexOf(">") + 1);
            report = report.Substring(0, report.IndexOf("<!-- End Body -->"));
            report = report.Replace("<BR>", "");

            writeToFile(report);

        }

        static void getIDs(string drugToSearch, string searchTerm, Dictionary<string, string>  OpVals, List<string> IDs)
        {
            string drugOpVal = OpVals[drugToSearch];
            string url = "https://erowid.org/experiences/exp.cgi?S1=" + drugOpVal + "&Str=" + searchTerm + "&Cellar=1&ShowViews=0&Cellar=1&Start=0&Max=100000";
            string source = getURLSource(url);

            while (source.IndexOf("exp.php?ID=") != -1)
            {
                source = source.Substring(source.IndexOf("exp.php?ID="));
                source = source.Substring(source.IndexOf("=") + 1);
                string id = source.Substring(0, source.IndexOf("\""));
                IDs.Add(id);
            }
        }

        static void scrapeOV(Dictionary<string, string> ov)
        {
            //restrict to drug section
            string source = getURLSource("https://erowid.org/experiences/exp_search.cgi");
            int start = source.IndexOf("option value=");
            int end = source.Substring(start).IndexOf("</select>");
            source = source.Substring(start, end);

            //init loop and skip ------All-------
            source = source.Substring(source.IndexOf(">") + 1);
            end = source.IndexOf("</option>");
            string drugName = source.Substring(0, end);


            for(int i = 0; i < 9999; i++)
            {
                start = source.IndexOf("option value=");
                
                //if we're at end, stop loop
                if (start == -1)
                    break;

                source = source.Substring(start);

                //get option value
                string valString = source.Substring(source.IndexOf("\"") + 1);
                valString = valString.Substring(0, valString.IndexOf("\""));
                int val = Int32.Parse(valString);

                //get name
                source = source.Substring(source.IndexOf(">") + 1);
                end = source.IndexOf("</option>");
                drugName = source.Substring(0, end);

                if(drugName != "111__use_this_one_empty")
                    ov.Add(drugName, valString);
            }
        }

        static void writeToFile(string content)
        {
            System.IO.File.WriteAllText(@"output.txt", content);
        }

        static string getURLSource(string url)
        {
            string urlAddress = url;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlAddress);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream receiveStream = response.GetResponseStream();
                StreamReader readStream = null;

                if (response.CharacterSet == null)
                {
                    readStream = new StreamReader(receiveStream);
                }
                else
                {
                    readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));
                }

                string data = readStream.ReadToEnd();

                response.Close();
                readStream.Close();

                return data;
            }
            else
            {
                return "couldn't get HTML source";
            }
        }
    }
}
