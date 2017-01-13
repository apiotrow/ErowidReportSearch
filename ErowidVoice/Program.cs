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

namespace ErowidVoice {
    public class Program
    {
        public static void Main()
        {
            Dictionary<string, int> OpVals = new Dictionary<string, int>();

            scrapeOV(OpVals);
            foreach (KeyValuePair<string, int> entry in OpVals)
            {
                Console.WriteLine(entry.Key + " " + entry.Value);
            }

            string drugToSearch = "Mushrooms";
            string searchTerm = "voice";
            int drugOpVal = OpVals[drugToSearch];
            string url = "https://erowid.org/experiences/exp.cgi?S1=" + drugOpVal + "&Str=" + searchTerm + "&Cellar=1&ShowViews=0&Cellar=1&Start=0&Max=100000";
            string source = getURLSource(url);
            writeToFile(source);
        }


        static void scrapeOV(Dictionary<string, int> ov)
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
                    ov.Add(drugName, val);
            }
        }

        static void writeToFile(string content)
        {
            StreamWriter file = new System.IO.StreamWriter("output.txt");
            file.WriteLine(content);
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
