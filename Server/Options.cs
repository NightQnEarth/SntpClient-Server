using CommandLine;

namespace Server
{
    public class Options
    {
        [Option('o',
            "seconds-offset",
            Default = 0,
            HelpText = "Seconds count to lie server.")]
        public int SecondsOffset { get; set; }

        [Option('t',
            "receive-timeout",
            Default = 1000 * 60 * 5,
            HelpText = "")]
        public int ReceiveTimeout { get; set; }
    }
}