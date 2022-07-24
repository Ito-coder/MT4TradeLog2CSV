// See https://aka.ms/new-console-template for more information

//MQL4\Logs のログからEAのlot設定値を読み込む。
//たぶんMT4再起動の際にログが作られる。再起動しないとダメ。

List<LogData_EA> logData_EAs = new();

logData_EAs.AddRange(ReadLog(GetLastFile(@"H:\マイドライブ\MetaTrader4\OANDA - MetaTrader1\MQL4\Logs")));
logData_EAs.AddRange(ReadLog(GetLastFile(@"H:\マイドライブ\MetaTrader4\OANDA - MetaTrader2\MQL4\Logs")));
logData_EAs.AddRange(ReadLog(GetLastFile(@"H:\マイドライブ\MetaTrader4\OANDA - MetaTrader3\MQL4\Logs")));
logData_EAs.AddRange(ReadLog(GetLastFile(@"H:\マイドライブ\MetaTrader4\OANDA - MetaTrader4\MQL4\Logs")));

Dictionary<int, string> dic = new();
foreach (LogData_EA logData in logData_EAs)
{
    if (logData.Magic.Length == 0) continue;
    if (logData.Lots.Length == 0) continue;
    string magic = logData.Magic[0];
    string lot = logData.Lots[0];
    if (logData.Name.StartsWith("MultiLogicShot_v"))
    {
        magic = logData.Magic[0];
        if (logData.Pair.StartsWith("GBPUSD")) magic = magic[..^1] + "2";
        if (logData.Pair.StartsWith("EURUSD")) magic = magic[..^1] + "4";
        lot = logData.Lots[0] + logData.Lots[1];
    }
    else if (logData.Name.StartsWith("EA_final_max_5pair"))
    {
        if (logData.Pair.StartsWith("GBPUSD")) magic = logData.Magic[2];
        if (logData.Pair.StartsWith("EURUSD")) magic = logData.Magic[3];
        if (logData.Pair.StartsWith("USDJPY")) magic = logData.Magic[4];
        if (logData.Pair.StartsWith("USDCHF")) magic = logData.Magic[5];
        if (logData.Pair.StartsWith("AUDUSD")) magic = logData.Magic[6];
        lot = logData.Lots[1];
    }

    dic[int.Parse(magic.Split('=')[1])] = lot;
}

string headerFilename = "LotSetting_header.csv";
var outFilename = "LotSetting.csv";
var header_text = File.ReadAllLines(headerFilename);
var header_split = header_text[header_text.Length - 2].Split(',').ToList();
var header_magic = header_split.GetRange(2, header_split.Count - 2).Select(a => int.Parse(a == "" ? "0" : a)).ToList();

System.Text.StringBuilder buff = new();
buff.AppendJoin(Environment.NewLine, header_text);//header

//buff.AppendLine();
//buff.Append("lot raw, ");
//foreach (var magic in header_magic)
//{
//    buff.Append(",");
//    if (dic.ContainsKey(magic)) buff.Append(dic[magic].Trim());
//}

buff.AppendLine();
buff.Append("lot, ");
foreach (var magic in header_magic)
{
    buff.Append(",");
    if (dic.ContainsKey(magic)) buff.Append(dic[magic].Split('=')[^1]);
}

File.WriteAllText(outFilename, buff.ToString());

return;


string GetLastFile(string dir)
{
    var files = Directory.GetFiles(dir);
    if (files.Length == 0) throw new Exception("ファイルなし");
    return files.OrderBy(a => File.GetLastWriteTime(a)).Last();
}

List<LogData_EA> ReadLog(string file)
{
    var res = new List<LogData_EA>();
    var lines = File.ReadAllLines(file);
    foreach (var line in lines)
    {
        var words_ = line.Split('\t');
        if (words_.Length != 3) continue;
        var words = words_[2].Split(':', ';');
        if (words.Length >= 2
            && words[0].EndsWith("inputs")
            && words[0].Contains(',')
            )
        {
            var data = new LogData_EA();
            data.Name = words[0];
            var comma = words[0].IndexOf(',');
            var sp = words[0].IndexOf(' ', comma);
            data.Pair = words[0].Substring(comma - 8, sp - comma + 8);
            data.Magic = words.Where(a => a.ToLower().Contains("magic")).ToArray();
            data.Lots = words.Where(a => a.ToLower().Contains("lot")).ToArray();
            res.Add(data);
        }
    }
    return res;
}

class LogData_EA
{
    public string Name = "";
    public string Pair = "";
    public string[] Magic = Array.Empty<string>();
    public string[] Lots = Array.Empty<string>();
}

