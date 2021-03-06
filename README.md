# MT4TradeLog2CSV

MT4のTradeLog(レポート)をEAのmagicごとに集計して、CSVに出力します。  
簡単にグラフ作れます。  

## Demo
![グラフ](doc/sample.png) 

# Usage
- 1.MT4でレポートの保存をします。  
![レポート](doc/SaveReport.png)  
- 2.ドラッグ・アンド・ドロップで起動します。  
![DnD](doc/DnD.png)  
- 3.集計されoutput.csvが作成されます。  
![output](doc/output.png)  
- 4.ご愛用の表計算ソフトに貼り付けて使います。  
![グラフ](doc/sample.png)  

- [visual studio 2022](https://visualstudio.microsoft.com/ja/vs/whatsnew/)でビルドしてます。  
html解析に[AngleSharp](https://github.com/AngleSharp/AngleSharp)使ってます。  

- 動作確認用のサンプルなど  
[レポートファイル](doc/sample.htm)  
[output.csv](doc/output.csv)  
[グラフ作成.ods](doc/sample.ods)  
グラフ作成済のods。  
Sheet1にoutput.csvをコピペしてます。  
使用例です。  
[header.csv](doc/header.csv)  
header.csvは自動生成されますが、右端に追加するだけなので、既存項目の編集は有効です。  
2行目にEAのコメントを拾ってますが、違っていたら直してください。  
再度実行することで反映されます。  
- 初期化について  
読み込んだトレードデータは「TradeLogs.bin」に蓄積されます。  
これにより、毎月や毎週の差分入力を可能にしています。  
初期化が必要な場合は「TradeLogs.bin」「header.csv」を削除してください。  

# Note
OANDA JAPANの口座で動作確認。  
スワップは他社口座では対応できない可能性大。  
基本自分用なので、細かいところは自身で修正して使ってください。  
要望があればいくらかは対応します。  
これまで、libreofficeでコピペ、コピペして作ってたけど、これで楽になります＾＾  

## Author
[Ito](https://github.com/Ito-coder)

