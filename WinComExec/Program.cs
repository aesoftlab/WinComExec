using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;     // MessageBoxを使用します。参照設定にも追加してください。

namespace WinComExec
{
    class Program
    {
        /// <summary>
        /// コマンドラインパラメータが "--help(-h)" を含んでいるばあいに表示するヘルプメッセージを定義します
        /// </summary>
        const string help = "";
        /// <summary>
        /// コマンドラインパラメータが "--debug(-d)" を含むばあい値をtrueに設定します
        /// </summary>
        static bool _debug = false;
        /// <summary>
        /// 機能を実装します
        /// </summary>
        /// <param name="args">--help(-h)または--debug(―d)が有効です</param>
        /// <returns>Read TypeにNone以外を指定したばあい、Resultで設定したパラメータに値を設定します</returns>
        static int Main(string[] args)
        {
            int stat = 0;
            try
            {
                // コマンドラインパラメータを解析します
                foreach (string arg in args)
                {
                    string a = arg.Trim().ToLower();
                    switch (a)
                    {
                        case "--help":
                        case "-h":
                            Console.Out.Write(help);
                            return 0;
                        case "--debug":
                        case "-d":
                            _debug = true;
                            break;
                        default:
                            throw new ArgumentException("無効なコマンドラインパラメータです\n" + arg);
                    }
                }
                // --helpオプションの指定がなく、このあとのReadToEndで何も入力がないと、アプリケーションは入力
                // 待ちのまま何もしません。 別スレッドを起動し、一定時間入力が無いばあいにあプログラムを強制終了
                // するようにします。
                System.Threading.Thread timeoutProc = new System.Threading.Thread(ReadToEndTimeoutProc);
                timeoutProc.Start();
                // 標準入力からのデータ入力完了を待ちます
                string src = Console.In.ReadToEnd();
                // 標準入力からのデータ入力が完了したのでスレッドを終了します
                timeoutProc.Abort();
                // (SAMPLE) srcをセパレータで分割、文字列配列化して、要素を逆順に並び替えたうえでCSVにします。
                string[] inputs = src.Split(new string[] { ",", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                string result = string.Join(",", inputs.Reverse());
                // 実行結果を標準出力に出力します
                Console.Out.Write(result);
                // コマンドラインパラメータで--debug(―d)の指定があるばあい、インプットとアウトプットをファイルに
                // 出力したうでメッセージボックスに表示します
                if (_debug)
                {
                    string logMessage = string.Format("IN:\n{0}\n\nOUT:\n{1}", src, result);
                    System.IO.File.WriteAllText("C:\\source.txt", logMessage);
                    MessageBox.Show(logMessage);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, typeof(Program).Namespace, MessageBoxButtons.OK, MessageBoxIcon.Error);
                stat = -1;
            }
            return stat;
        }
        /// <summary>
        /// ReadToEndの入力タイムアウト処理を実装します
        /// </summary>
        /// <param name="obj"></param>
        private static void ReadToEndTimeoutProc(object obj)
        {
            // 5秒間で入力が完了しなければメッセージボックスでエラーを表示します。
            System.Threading.Thread.Sleep(5000);
            MessageBox.Show("文字列の読み込みがタイムアウトしました。\nヘルプを表示するにはコマンドラインオプションで --help または -h を指定してください。",
                typeof(Program).Namespace, MessageBoxButtons.OK, MessageBoxIcon.Error);
            Environment.Exit(258);  // =WAIT_TIMEOUT;
        }
    }
}
