// See https://aka.ms/new-console-template for more information


namespace MT4TradeLog2CSV // Note: actual namespace depends on the project name.
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var outFilename = "output.csv";
            var tradeListFilename = "TradeLogs.bin";

            try
            {
                var tradeLogManager = new TradeLogManager();

                tradeLogManager.Load(tradeListFilename);
                Console.WriteLine("load:" + tradeLogManager.TradeDatas.Count + "count");

                for (var i = 0; i < args.Length; ++i)
                {
                    Console.WriteLine("read:" + args[i]);
                    tradeLogManager.ProfitSum = 0;
                    tradeLogManager.AppendMT4TradeLog(args[i]);
                    Console.WriteLine("ProfitSum:" + tradeLogManager.ProfitSum + "  Closed P/L:" + tradeLogManager.Closed_PL);
                    if (tradeLogManager.ProfitSum != tradeLogManager.Closed_PL)
                    {
                        Console.WriteLine("ProfitSum 不一致");
                        Console.ReadKey();
                    }
                }

                tradeLogManager.Save(tradeListFilename);
                Console.WriteLine("save:" + tradeLogManager.TradeDatas.Count + "count");

                tradeLogManager.UpdateHeader();
                Console.WriteLine("UpdateHeader:");

                tradeLogManager.OutCSV(outFilename);
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
