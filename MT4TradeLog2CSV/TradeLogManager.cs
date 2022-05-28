using AngleSharp.Html.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace MT4TradeLog2CSV
{
    public class TradeLogManager
    {
        public List<TradeLog> TradeDatas = new List<TradeLog>();
        public TradeLogManager() { }
        public void Save(string filename)
        {//save
            try
            {
                var serializer = new DataContractSerializer(TradeDatas.GetType());
                using var stream = new FileStream(filename, FileMode.Create);

                var writer = XmlDictionaryWriter.CreateBinaryWriter(stream);
                serializer.WriteObject(writer, TradeDatas);
                writer.Flush();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        public void Load(string filename)
        {//Load
            try
            {
                if (File.Exists(filename) == false) return;
                var serializer = new DataContractSerializer(TradeDatas.GetType());
                using var stream = new FileStream(filename, FileMode.Open);

                var reader = XmlDictionaryReader.CreateBinaryReader(stream, XmlDictionaryReaderQuotas.Max);
                var temp = serializer.ReadObject(reader);
                if (temp != null) TradeDatas = (List<TradeLog>)temp;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        public void AppendMT4TradeLog(string filename,out int ProfitSum,out int Closed_PL)
        {
            ProfitSum = 0;
            Closed_PL = 0;
            var html = File.ReadAllText(filename, System.Text.Encoding.UTF8);
            var doc = new HtmlParser().ParseDocument(html);

            var data = new TradeLog();
            foreach (var tr in doc.Body.GetElementsByTagName("tr"))
            {
                if (tr.TextContent.Contains("Open Trades:")) return;//以下のデータは違う。
                var td = tr.GetElementsByTagName("td");
                if (td.Length == 14
                    && (td[2].TextContent == "sell" || td[2].TextContent == "buy")
                    )
                {//売買1行目
                    data = new TradeLog();
                    data.Ticket = int.Parse(td[0].TextContent);
                    data.OpenTime = DateTime.Parse(td[1].TextContent);
                    data.Profit = int.Parse(td[13].TextContent.Replace(" ", ""));
                    data.Lot = double.Parse(td[3].TextContent);
                    ProfitSum += data.Profit;
                }
                else if (td.Length == 3
                    && td[0].GetAttribute("colspan") == "9"
                    )
                {//売買2行目//magic//comment
                    data.Magic = int.Parse(td[1].TextContent);
                    data.Comment = td[2].TextContent;
                    //if (TradeDatas.Any(a => a.Ticket == data.Ticket) == false) TradeDatas.Add(data);
                    //古いデータを削除して更新する。
                    TradeDatas.RemoveAll(a => a.Ticket == data.Ticket);
                    TradeDatas.Add(data);
                }
                //balanceとして資金移動含むデータならば、おそらく正確なデータが得られる。
                //スワップ、預け金の利息？、資金移動の分別は困難と思われる。
                //Primary 退避資金にも利息が付いていると思われ、これのデータは取得していない。
                //一旦、スワップなどのデータは入れないことにした。コメントアウト。
                //else if (td.Length == 5
                //    && td[2].TextContent == "balance"//これだけだと資金移動含む
                //    //&& (td[3].TextContent == "Financing" || td[3].TextContent == "Interest")
                //    )
                //{//スワップなど？
                //    data = new TradeLog();
                //    data.Ticket = int.Parse(td[0].TextContent);
                //    data.OpenTime = DateTime.Parse(td[1].TextContent);
                //    data.Profit = int.Parse(td[4].TextContent.Replace(" ", ""));
                //    data.Comment = td[3].TextContent;
                //    data.Magic = data.Comment.GetHashCode();
                //    switch (data.Comment) 
                //    {
                //        case "Interest":
                //        case "Financing":
                //        case "CLIENT_FUNDING":
                //            //data.Magic = 0;
                //            //break;
                //        case "Balance update":
                //        case "AddFunds":
                //        case "DelFunds":
                //        case "AddFundsTransfer":
                //        case "DelFundsTransfer":
                //        case "ACCOUNT_TRANSFER":
                //            continue;//foreach
                //    }
                //    if (TradeDatas.Any(a => a.Ticket == data.Ticket) == false) TradeDatas.Add(data);
                //}
                else if (td.Length == 2
                    && td[0].TextContent == "Closed P/L:"
                    )
                {//売買2行目//magic//comment
                    Closed_PL = int.Parse(td[1].TextContent.Replace(" ", ""));
                    return;//これ以下のデータは違う。
                }
            }
        }
        public void UpdateHeader(string headerFilename = "header.csv")
        {
            Dictionary<int, List<TradeLog>> magic_map = new();

            foreach (var trade in TradeDatas)
            {
                if (magic_map.ContainsKey(trade.Magic) == false) magic_map[trade.Magic] = new();
                magic_map[trade.Magic].Add(trade);
            }

            //ヘッダ追加
            if (File.Exists(headerFilename) == false) File.WriteAllText(headerFilename, "magic,-" + Environment.NewLine + "Date,sum");
            var header_text = File.ReadAllLines(headerFilename);
            var header_split = header_text[header_text.Length - 2].Split(',').ToList();
            var header_magic = header_split.GetRange(2, header_split.Count - 2).Select(a => int.Parse(a)).ToList();

            foreach (var magic_pair in magic_map.OrderBy(a => a.Key))
            {
                if (header_magic.Contains(magic_pair.Key) == false)
                {
                    header_magic.Add(magic_pair.Key);
                    header_text[header_text.Length - 1] += "," + magic_pair.Value.First().Comment;
                    header_text[header_text.Length - 2] += "," + magic_pair.Key;
                }
            }
            File.WriteAllLines(headerFilename, header_text);
        }
        public void OutCSV(string outFilename, string headerFilename = "header.csv")
        {
            Dictionary<DateTime, List<TradeLog>> date_map = new();
            Dictionary<int, List<TradeLog>> magic_map = new();

            foreach (var trade in TradeDatas)
            {
                if (date_map.ContainsKey(trade.OpenTime.Date) == false) date_map[trade.OpenTime.Date] = new();
                date_map[trade.OpenTime.Date].Add(trade);
                if (magic_map.ContainsKey(trade.Magic) == false) magic_map[trade.Magic] = new();
                magic_map[trade.Magic].Add(trade);
            }

            //最大値 最小値 初期値
            //Dictionary<int, DateTime> magic_max_date = new();
            //Dictionary<int, DateTime> magic_min_date = new();
            Dictionary<int, int> profits_sum = new();
            int profit_sum_all = 0;

            //ヘッダ読み込み
            var header_text = File.ReadAllLines(headerFilename);
            var header_split = header_text[header_text.Length - 2].Split(',').ToList();
            var header_magic = header_split.GetRange(2, header_split.Count - 2).Select(a => int.Parse(a)).ToList();

            foreach (var magic in header_magic)
            {
                profits_sum[magic] = 0;
            }

            //sum出力
            StringBuilder buff = new();
            buff.AppendJoin(Environment.NewLine, header_text);//header
            buff.AppendLine();

            foreach (var trades in date_map.OrderBy(a => a.Key))
            {
                buff.Append(trades.Key.ToString("d"));//date
                Dictionary<int, int> profits = new();
                int profit_sum_day = 0;
                foreach (var trade in trades.Value)
                {
                    profit_sum_day += trade.Profit;
                    if (profits.ContainsKey(trade.Magic) == false) profits[trade.Magic] = 0;
                    profits[trade.Magic] += trade.Profit;
                }

                profit_sum_all += profit_sum_day;
                buff.Append("," + profit_sum_all);//profit_sum

                foreach (var magic in header_magic)
                {
                    buff.Append(",");
                    if (profits.ContainsKey(magic)) profits_sum[magic] += profits[magic];
                    //if (magic_min_date[magic] <= trades.Key && trades.Key <= magic_max_date[magic])
                    if (profits_sum[magic] != 0) buff.Append(profits_sum[magic]);//profit
                }
                buff.AppendLine();
            }
            File.WriteAllText(outFilename, buff.ToString());

            //lot出力
            buff = new();
            buff.AppendJoin(Environment.NewLine, header_text);//header
            buff.AppendLine();

            foreach (var trades in date_map.OrderBy(a => a.Key))
            {
                buff.Append(trades.Key.ToString("d"));//date
                Dictionary<int, double> lots = new();
                foreach (var trade in trades.Value)
                {
                    if (lots.ContainsKey(trade.Magic) == false) lots[trade.Magic] = 0;
                    lots[trade.Magic] = trade.Lot;
                }

                buff.Append(",");//sumダミー

                foreach (var magic in header_magic)
                {
                    buff.Append(",");
                    if (lots.ContainsKey(magic)) buff.Append(lots[magic]);
                }
                buff.AppendLine();
            }
            File.WriteAllText(outFilename.Replace(".", "_lot."), buff.ToString());
        }
    }
    public class TradeLog
    {
        public int Ticket;
        public DateTime OpenTime;
        public int Profit;
        public int Magic;
        public string Comment = "";
        public double Lot;
    }
}
