// See https://aka.ms/new-console-template for more information


namespace MT4TradeLog2CSV // Note: actual namespace depends on the project name.
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var outFilename = "output.csv";
            var tradeListFilename = "TradeLogs.bin";

            var TradeDatas = new TradeLogManager(tradeListFilename);
            Console.WriteLine("load:" + TradeDatas.TradeDatas.Count + "count");
            try
            {
                for (var i = 0; i < args.Length; ++i)
                {
                    Console.WriteLine("read:" + args[i]);
                    TradeDatas.AppendMT4TradeLog(args[i]);
                }

                TradeDatas.Save(tradeListFilename);
                Console.WriteLine("save:" + TradeDatas.TradeDatas.Count + "count");

                TradeDatas.UpdateHeader();
                Console.WriteLine("UpdateHeader:");

                TradeDatas.OutCSV(outFilename);
                Console.WriteLine("outCSV:" + outFilename);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadKey();
            }
        }
    }

}
