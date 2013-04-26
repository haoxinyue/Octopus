using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Octopus.RFID;
using Octopus.RFID.Filter;
using Octopus.Config;
using System.Threading.Tasks;

namespace Octopus.Sample.RFID
{
    public class TestCase1
    {
        public void RunTest()
        {
            Task startServerTask = new Task(() => StartServerWithConfig());
            startServerTask.Start();
            startServerTask.Wait();
        }

        public void StartServer()
        {
            RFIDConfig config = new RFIDConfig();
            config.ConnectionStrings.Add("10.1.5.190");
            config.PrividerName = "Impinj";
            config.ReaderPower = 20;

            RSSIFilter rssIFilter = new RSSIFilter("RSSIFilter", -100);
            DuplicateFilter duplicateFilter = new DuplicateFilter("DuplicateFilter", 30);
            rssIFilter.AddNextFilter(duplicateFilter);

            RFIDChannel channel = new RFIDChannel("RFIDChannel", config, null);
            RFIDInterpreter interpreter = new RFIDInterpreter("RFIDInterpreter");
            interpreter.FirstFilter = rssIFilter;

            RFIDAdapter adapter = new RFIDAdapter("TagInfoAdapter", channel, interpreter, Program.ShowEnvelop);

            adapter.Setup();
        }

        public void StartServerWithConfig()
        {
            OctopusConfig oc = new OctopusConfig();
            oc.LoadRFIDAdapter("RFID\\TestCase1\\TestCase1.xml");

            foreach (var item in oc.Adapters)
            {
                item.Value.AddEnvelopHandler(Program.ShowEnvelop);
                item.Value.Setup();
            }
        }
    }
}
