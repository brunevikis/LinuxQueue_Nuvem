using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LinuxQueueCore;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace LinuxQueueApi
{
    public class Program
    {
        public static void Main(string[] args)
        {

            if (args.Contains("auto"))
            {
                LinuxQueueCore.QueueFolders.RegisterFolder();
                AutoRun();
            }
            else if (args.Contains("autoEncad"))
            {
                LinuxQueueCore.QueueFolders.RegisterFolder();
                AutoRun_Encad();
            }
            else if (args.Contains("callback") && args.Length == 2)
            {
                QueueFolders.RegisterFolder();
                Callback(args[1]);
            }
            else if (args.Contains("detail") && args.Length == 2)
            {
                LinuxQueueCore.QueueFolders.RegisterFolder();
                Detail(args[1]);
            }
            else if (args.Contains("cleanup"))
            {
                LimparAntigos();
            }
            else if (args.Contains("result"))//result "teste"
            {
                Get_Resultado(args[1], true);

            }
            else if (args.Length == 0)
            {
                BuildWebHost(args).Run();
            }

            else
                Console.WriteLine("Parametro inválido");
        }

        private static void Callback(string name)
        {
            QueueController ctl = new QueueController();

            var comms = ctl.ReadComms();

            var l = comms.Where(x => x.CommandName == name).FirstOrDefault();

            if (l != null && l.EnviarEmail)
            {

                var sumario = Directory.GetFiles(l.WorkingDirectory, "sumario.rv*");
                var txtSum = "";

                if (sumario.Length > 0)
                {
                    var cmo = File.ReadAllText(sumario[0]);

                    var i = cmo.IndexOf("CUSTO MARGINAL");

                    if (i > 0)
                    {
                        var f = cmo.IndexOf("Leve", i);
                        txtSum = cmo.Substring(i, f - i + 4);

                        txtSum = txtSum.Replace("         ", "");
                    }

                    txtSum.Replace("\r", "").Replace("\n", "</br>");
                }

                var bodyHtml = $"<html><head><meta http - equiv = 'Content-Type' content = 'text/html; charset=UTF-8' ></head><body> " +
       $"<h1>{name}</h1>" +
       $"<p><strong>Caminho: </strong>{l.WorkingDirectory}</p>" +
       $"<p><pre>{txtSum}</pre></p>" +
       $"</body></html>";

                var toAddr = l.User == "AutoRun" ? "pedro.modesto@enercore.com.br;natalia.biondo@enercore.com.br;bruno.araujo@enercore.com.br;thamires.baptista@enercore.com.br"
                    : l.User + "@enercore.com.br";



#if DEBUG
                //toAddr = "alex.marques@cpas.com.br";
#endif

                SendMail(bodyHtml, toAddr);
            }


        }
        private static void Detail(string name)
        {


            QueueController ctl = new QueueController();


            var comms = ctl.ReadComms();


            var l = comms.Where(x => x.CommandName == name).FirstOrDefault();

            if (l != null)
            {
                Console.WriteLine($"WD: {l.WorkingDirectory}");
                Console.WriteLine($"FD: {l.FolderType}");
            }


        }

        public static IWebHost BuildWebHost(string[] args) =>

#if DEBUG

            WebHost.CreateDefaultBuilder(args)
                .UseKestrel()
                .UseUrls("http://0.0.0.0:5015/", "http://127.0.0.1:0/")
                .UseStartup<Startup>()
                .Build();

#else

            WebHost.CreateDefaultBuilder(args)
                .UseKestrel()
                .UseUrls("http://0.0.0.0:5014/", "http://127.0.0.1:0/")
                .UseStartup<Startup>()
                .Build();

#endif



        public static void AutoRun()
        {
            var configs = Model.Config.get();

            //listar requisições em auto;            
            var autoFolder = Path.Combine(LinuxQueueCore.QueueFolders.rootPath, "auto");
            var diretorios = Directory.GetDirectories(autoFolder);
            System.Threading.Thread.Sleep(5000);

            foreach (var config in configs)
            {

#if DEBUG
                System.Console.WriteLine(config.WorkingDirectory);
#endif                

                //fazer copia da pasta com o deck
                var deckOriginal = config.WorkingDirectory;

                var textRV = "";
                var RV = Directory.GetFiles(deckOriginal, "*.*", SearchOption.TopDirectoryOnly)
                    .FirstOrDefault(x => Path.GetFileName(x).ToLowerInvariant() == "caso.dat");

#if DEBUG
                System.Console.WriteLine(RV);
#endif


                if (RV != null)
                {
                    textRV = File.ReadLines(RV).First().Trim();
                }
                else return;


                string diretorioParaSerExecutado = null;

                if (diretorios.Length == 0) return;
                else
                {
                    foreach (var diretorio in diretorios)
                    {
#if DEBUG
                        System.Console.WriteLine(diretorio);
#endif

                        if (File.Exists(Path.Combine(diretorio, "prevs." + textRV)))
                        {
                            diretorioParaSerExecutado = diretorio;
                            break;
                        }
                    }
                }

                if (string.IsNullOrEmpty(diretorioParaSerExecutado)) continue;

                var autoRunName = (new DirectoryInfo(diretorioParaSerExecutado)).Name;

                var novoDeck = Path.Combine(Path.GetDirectoryName(deckOriginal), autoRunName);

                Console.WriteLine("autoRunName = " + novoDeck);
                Console.WriteLine("Base deck = " + deckOriginal);
                Console.WriteLine("Novo deck = " + novoDeck);

                if (System.IO.Directory.Exists(novoDeck))
                {
                    Console.WriteLine("Ja exitente, nao sera processado");
                    Directory.Delete(diretorioParaSerExecutado, true);
                    return;
                }
                else if (!System.IO.Directory.Exists(deckOriginal))
                {
                    Console.WriteLine("Deck base nao encontrado");
                    Directory.Delete(diretorioParaSerExecutado, true);
                    return;
                }

                Directory.CreateDirectory(novoDeck);

                foreach (var file in Directory.GetFiles(deckOriginal))// fazer a substituiçao do dadger
                {
                    File.Copy(file, Path.Combine(novoDeck, Path.GetFileName(file)));
                }

                foreach (var file in Directory.GetFiles(diretorioParaSerExecutado))
                {
                    File.Copy(file, Path.Combine(novoDeck, Path.GetFileName(file)), true);
                }

                Directory.Delete(diretorioParaSerExecutado, true);

                //agendar execucao e aguardar
                QueueController ctl = new QueueController();

                CommItem comm = new CommItem()
                {

                    Cluster = Cluster.Default,
                    Command = config.Command,
                    User = "AutoRun",
                    WorkingDirectory = novoDeck,
                    CommandName = autoRunName + "_" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                    EnviarEmail = true

                };
                ctl.Enqueue(comm);
            }
        }


        public static void AutoRun_Encad()
        {
            var configs = Model.ConfigEncad.get();

            //listar requisições em auto;            
            var autoFolder = Path.Combine(LinuxQueueCore.QueueFolders.rootPath, "auto");
            var diretorios = Directory.GetDirectories(autoFolder);
            System.Threading.Thread.Sleep(5000);

            foreach (var config in configs)
            {

#if DEBUG
                System.Console.WriteLine(config.WorkingDirectory);
#endif                

                //fazer copia da pasta com o deck
                var deckOriginal = config.WorkingDirectory;

                var textRV = "";
                var RV = Directory.GetFiles(deckOriginal, "*.*", SearchOption.TopDirectoryOnly)
                    .FirstOrDefault(x => Path.GetFileName(x).ToLowerInvariant() == "caso.dat");

#if DEBUG
                System.Console.WriteLine(RV);
#endif


                if (RV != null)
                {
                    textRV = File.ReadLines(RV).First().Trim();
                }
                else return;


                string diretorioParaSerExecutado = null;

                if (diretorios.Length == 0) return;
                else
                {
                    foreach (var diretorio in diretorios)
                    {
#if DEBUG
                        System.Console.WriteLine(diretorio);
#endif

                        if (File.Exists(Path.Combine(diretorio, "prevs." + textRV)))
                        {
                            diretorioParaSerExecutado = diretorio;
                            break;
                        }
                    }
                }

                if (string.IsNullOrEmpty(diretorioParaSerExecutado)) continue;

                var autoRunName = (new DirectoryInfo(diretorioParaSerExecutado)).Name;

                var novoDeck = Path.Combine(Path.GetDirectoryName(deckOriginal), autoRunName);

                Console.WriteLine("autoRunName = " + novoDeck);
                Console.WriteLine("Base deck = " + deckOriginal);
                Console.WriteLine("Novo deck = " + novoDeck);

                if (System.IO.Directory.Exists(novoDeck))
                {
                    Console.WriteLine("Ja exitente, nao sera processado");
                    Directory.Delete(diretorioParaSerExecutado, true);
                    return;
                }
                else if (!System.IO.Directory.Exists(deckOriginal))
                {
                    Console.WriteLine("Deck base nao encontrado");
                    Directory.Delete(diretorioParaSerExecutado, true);
                    return;
                }

                Directory.CreateDirectory(novoDeck);

                foreach (var file in Directory.GetFiles(deckOriginal))// fazer a substituiçao do dadger
                {
                    File.Copy(file, Path.Combine(novoDeck, Path.GetFileName(file)));
                }

                foreach (var file in Directory.GetFiles(diretorioParaSerExecutado))
                {
                    File.Copy(file, Path.Combine(novoDeck, Path.GetFileName(file)), true);
                }

                Directory.Delete(diretorioParaSerExecutado, true);

                //agendar execucao e aguardar
                QueueController ctl = new QueueController();

                CommItem comm = new CommItem()
                {

                    Cluster = Cluster.Default,
                    Command = config.Command,
                    User = "AutoRun",
                    WorkingDirectory = novoDeck,
                    CommandName = autoRunName + "_" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                    EnviarEmail = true

                };
                ctl.Enqueue(comm);
            }
        }
        public static void RunnerLinux()
        {
            QueueController.ReadConfig();
            List<Cluster> Clusters_indisp = new List<Cluster>();

            QueueController ctl = new QueueController();
            var comms = ctl.ReadComms();
            var clusters = ctl.ReadClusters();
            var command_auth = ctl.ReadAuth();

            var Files_Queue = comms.Where(x => x.FolderType.ToString() == "Queue").OrderBy(x => x.Order).ToList();

            var Files_Run = comms.Where(x => x.FolderType.ToString() == "Running").OrderBy(x => x.Order).ToList();


            foreach (var file in Files_Run)
            {
                Clusters_indisp.Add(file.Cluster);

            }

            var Clusters_disp = clusters.Where(y => ((Clusters_indisp.Where(x => x.Alias == y.Alias).Count()) == 0) && y.Enabled == true).ToList();
            Cluster cluster_exec;
            foreach (var caso in Files_Queue)
            {
                if (command_auth.Where(x => x.Command == caso.Command).Count() != 0)
                {
                    var alias_cluster = command_auth.Where(x => x.Command == caso.Command).FirstOrDefault().Cluster;
                    cluster_exec = Clusters_disp.Where(x => x.Alias.Contains(alias_cluster)).FirstOrDefault();

                    if (cluster_exec != null)
                    {
                        if (File.Exists(Path.Combine(@"L:\Teste_Fila_Dotnet\Status", cluster_exec.Host.ToString())))
                        {
                            var teste = "Está Ligado";

                        }
                        else
                        {
                            var teste = "Chama AutoShutdown";
                            break;
                        }
                    }

                }
            }
            var saiu = 0;

        }
        public static void LimparAntigos()
        {
            try
            {
                #region Apaga pastas e arquivos obsoletas

                List<string> diretoriosExcluidos = new List<string>();
                List<string> arquivosExcluidos = new List<string>();

                var diretorios = Directory.GetDirectories("/home/compass/sacompass/previsaopld/5_encadeado");//L:\\");

                if (diretorios.Length > 0)
                    foreach (var diretorio in diretorios)
                    {
                        DirectoryInfo diretorioInfo = new DirectoryInfo(diretorio);
                        if (diretorioInfo.CreationTime < DateTime.Today.AddDays(-45))
                        {
                            try
                            {
                                Directory.Delete(diretorio, true);
                                diretoriosExcluidos.Add(diretorio);
                            }
                            catch { }
                        }

                        else
                        {
                            var diretoriosFilho = Directory.GetDirectories(diretorio);

                            if (diretoriosFilho.Length > 0)
                                foreach (var diretorioFilho in diretoriosFilho)
                                {
                                    var diretoriosNeto = Directory.GetDirectories(diretorioFilho);

                                    if (diretoriosNeto.Length > 0)
                                        foreach (var dirNeto in diretoriosNeto)
                                        {
                                            var files = Directory.GetFiles(dirNeto);

                                            if (files.Length > 0)
                                                foreach (var file in files)
                                                {
                                                    if (file.EndsWith("cortes.dat") || file.EndsWith("vazinat.dat") || file.EndsWith("forward.zip") || file.EndsWith("cortese.zip"))
                                                    {
                                                        FileInfo fileInfo = new FileInfo(file);
                                                        if (fileInfo.LastWriteTime < DateTime.Today.AddDays(-20))
                                                        {
                                                            try
                                                            {
                                                                File.Delete(file);
                                                                arquivosExcluidos.Add(file);
                                                            }
                                                            catch { }
                                                        }
                                                    }
                                                }
                                        }
                                    else
                                    {
                                        var files = Directory.GetFiles(diretorioFilho);

                                        if (files.Length > 0)
                                            foreach (var file in files)
                                            {
                                                if (file.EndsWith("cortes.dat") || file.EndsWith("vazinat.dat") || file.EndsWith("forward.zip") || file.EndsWith("cortese.zip"))
                                                {
                                                    FileInfo fileInfo = new FileInfo(file);
                                                    if (fileInfo.LastWriteTime < DateTime.Today.AddDays(-15))
                                                    {
                                                        try
                                                        {
                                                            File.Delete(file);
                                                            arquivosExcluidos.Add(file);
                                                        }
                                                        catch { }

                                                    }
                                                }
                                            }
                                    }
                                }
                        }
                    }

                var bodyHtml = $"<html><head><meta http - equiv = 'Content-Type' content = 'text/html; charset=UTF-8' ></head><body> " +
           $"<p><strong>Quantidade de itens excluidos: </strong>{arquivosExcluidos.Count}</p>" +
           $"<p><strong>Quantidade de pastas excluidas: </strong>{diretoriosExcluidos.Count}</p>";

                if (arquivosExcluidos.Count > 0)
                    foreach (var arq in arquivosExcluidos)
                        bodyHtml += $"<p><strong>Caminho do arquivo excluido: </strong>{arq}</p>";

                if (diretoriosExcluidos.Count > 0)
                    foreach (var dir in diretoriosExcluidos)
                        bodyHtml += $"<p><strong>Caminho da pasta excluida: </strong>{dir}</p>";


                bodyHtml += $"<p><pre></pre></p>" + $"</body></html>";

                SendMail(bodyHtml, "bruno.araujo@enercore.com.br", "Exclusão de pastas e arquivos");

                #endregion
            }
            catch (Exception e)
            {
                var bodyHtml = $"<html><head><meta http - equiv = 'Content-Type' content = 'text/html; charset=UTF-8' ></head><body> " +
           $"<p><strong>Erro no LinuxQueueApi.LimparAntigos() </strong></p>" +
           $"<p><strong>Erro: {e.Message}</p>" +
           $"<p><pre></pre></p>" + $"</body></html>";

                SendMail(bodyHtml, "bruno.araujo@enercore.com.br", "Exclusão de pastas e arquivos");
            }
        }

        public static void SendMail(string body, string emails = "pedro.modesto@enercore.com.br;natalia.biondo@enercore.com.br;bruno.araujo@enercore.com.br;thamires.baptista@enercore.com.br", string subject = "Execução automática")
        {

            System.Net.Mail.MailMessage msg = new System.Net.Mail.MailMessage();

            msg.IsBodyHtml = true;

            msg.BodyEncoding = System.Text.Encoding.UTF8;

            msg.Body = body;

            msg.Subject = subject;

            msg.Sender = msg.From = new System.Net.Mail.MailAddress("cpas.robot@gmail.com");

            msg.ReplyToList.Add(new System.Net.Mail.MailAddress("bruno.araujo@enercore.com.br"));

            // var emails = "douglas.canducci@cpas.com.br;pedro.modesto@cpas.com.br;diana.lima@cpas.com.br;natalia.biondo@cpas.com.br;bruno.araujo@cpas.com.br;alex.marques@cpas.com.br";

            foreach (var to in emails.Split(new char[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries))
            {
                msg.To.Add(to);
            }


            System.Net.Mail.SmtpClient cli = new System.Net.Mail.SmtpClient();

            cli.Host = "smtp.gmail.com";
            cli.Port = 587;
            cli.Credentials = new System.Net.NetworkCredential("cpas.robot@gmail.com", "cp@s9876");

            cli.EnableSsl = true;

            cli.Send(msg);
        }

        private static void Get_Resultado_CV()
        {
            var toAddr = "bruno.araujo@enercore.com.br";

            var bodyHtml = $"<html><head><meta http - equiv = 'Content-Type' content = 'text/html; charset=UTF-8' ></head><body> <br>";
            QueueController ctl = new QueueController();

            var comms = ctl.ReadComms();

            var l = comms.Where(x => x.CommandName.Contains(DateTime.Today.ToString("yyyyMMdd") + "_CV")).ToList();

            var l_cv = l.Where(x => x.CommandName.Contains(DateTime.Today.ToString("yyyyMMdd") + "_CV_ACOMPH_FUNC") && !x.CommandName.Contains("d-1"));
            var l_cv2 = l.Where(x => x.CommandName.Contains(DateTime.Today.ToString("yyyyMMdd") + "_CV2_ACOMPH_FUNC") && !x.CommandName.Contains("d-1"));

            if (l_cv.Count() == 0)
            {
                var l_cv_D1 = l.Where(x => x.CommandName.Contains(DateTime.Today.ToString("yyyyMMdd") + "_CV_ACOMPH_FUNC_d-1"));
                var l_cv2_D1 = l.Where(x => x.CommandName.Contains(DateTime.Today.ToString("yyyyMMdd") + "_CV2_ACOMPH_FUNC_d-1"));

                if (((l_cv_D1.Count() >= 1) && (l_cv2_D1.Count() >= 1) && (DateTime.Today.DayOfWeek != DayOfWeek.Thursday)) || ((l_cv_D1.Count() >= 3) && (l_cv2_D1.Count() >= 3)))
                {
                    foreach (var cv in l_cv_D1)
                    {
                        var table = Program.Tabela(cv);
                        bodyHtml = bodyHtml + table;
                    }
                    foreach (var cv in l_cv2_D1)
                    {
                        var table = Program.Tabela(cv);
                        bodyHtml = bodyHtml + table;
                    }

                }
                SendMail(bodyHtml, toAddr, "Teste Resultados");
            }
            else if (((l_cv.Count() >= 1) && (l_cv2.Count() >= 1) && (DateTime.Today.DayOfWeek != DayOfWeek.Thursday)) || ((l_cv.Count() >= 3) && (l_cv2.Count() >= 3)))
            {
                foreach (var cv in l_cv)
                {
                    var table = Program.Tabela(cv);
                    bodyHtml = bodyHtml + table;
                }
                foreach (var cv in l_cv2)
                {
                    var table = Program.Tabela(cv);
                    bodyHtml = bodyHtml + table;
                }
                SendMail(bodyHtml, toAddr, "Teste Resultados");
            }
        }

        public static string Tabela(CommItem Caso)
        {
            var DadosSE = Caso.pldSE.Split(';');
            var DadosS = Caso.pldS.Split(';');
            var DadosNE = Caso.pldNE.Split(';');
            var DadosN = Caso.pldN.Split(';');

            var table_html =
                $"<br>" +
                $"<h2>{Caso.CommandName.ToString()}</h2>" +
                $"<table  border = '1'>" +
   $"<thead>" +

   $"<tr>" +
   $"<th > Submercado </th>" +
   $"<th > CMO </th>" +
   $"<th > PLD </th>" +
   $"<th > EARM </th>" +
   $"</tr>" +
   $"</thead>" +
   $"<tbody>" +
   $"<tr>" +
   $"<td > Sudeste </td>" +
   $"<td > {DadosSE[0]} </td>" +
   $"<td > {DadosSE[1]} </td>" +
   $"<td > {DadosSE[2]} </td>" +
   $"</tr>" +


   $"<tr>" +
   $"<td > Sul </td>" +
   $"<td > {DadosS[0]} </td>" +
   $"<td > {DadosS[1]} </td>" +
   $"<td > {DadosS[2]} </td>" +
   $"</tr>" +

   $"<tr>" +
   $"<td > Nordeste </td>" +
   $"<td > {DadosNE[0]} </td>" +
   $"<td > {DadosNE[1]} </td>" +
   $"<td > {DadosNE[2]} </td>" +
   $"</tr>" +

   $"<tr>" +
   $"<td > Norte </td>" +
   $"<td > {DadosN[0]} </td>" +
   $"<td > {DadosN[1]}</td>" +
   $"<td > {DadosN[2]} </td>" +
   $"</tr>" +
   $"</tbody>" +

   $"</table>" +
   $"<br>";
            return table_html;
        }


        public static string Get_Resultado(string name, Boolean Auto = false)
        {
            //   string dir = @"L:\Teste_Fila_Dotnet\20201209_CV_2000";


            List<Resu_PLD_Mensal> Resu = new List<Resu_PLD_Mensal>();

            double PLD = 0;
            double Soma_CMO = 0;
            double Soma_Horas = 0;

            string tipo = null;

            QueueController ctl = new QueueController();

            var comms = ctl.ReadComms();

            var l = comms.Where(x => x.CommandName == name).FirstOrDefault();

            if (l == null)
            {
                comms = ctl.ReadComms(-30);
                l = comms.Where(x => x.CommandName == name).FirstOrDefault();
            }


            if (l != null)
            // if (dir != null)
            {
                try
                {
                    var dir_split = l.WorkingDirectory.Split('/');

                    var dir = l.WorkingDirectory.Replace("/home/producao/PrevisaoPLD/", "/home/compass/sacompass/previsaopld/");

                    //////////////////////////////////////
                    var caso = Directory.GetFiles(dir, "caso.dat");

                    var rv = File.ReadAllLines(caso[0]);

                    var dadger_file = Directory.GetFiles(dir, "dadger." + rv[0]);
                    if (dadger_file.Count() > 0)
                    {
                        var dadger = File.ReadAllLines(dadger_file[0]);
                        DateTime dt_estudo = DateTime.Today;



                        foreach (var linha in dadger)
                        {
                            if (linha.StartsWith("DT"))
                            {
                                var dados = linha.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                                dt_estudo = new DateTime(int.Parse(dados[3]), int.Parse(dados[2]), int.Parse(dados[1]));
                            }
                            else if (linha.StartsWith("& NO. SEMANAS"))
                            {
                                var dados = linha.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                                if (int.Parse(dados[8]) != 0)
                                {
                                    tipo = "semanal";
                                    dt_estudo = dt_estudo.AddDays(8);
                                }
                                else
                                {
                                    tipo = "mensal";
                                }
                            }
                        }

                        /////////////////////////////////////////



                        //var dir_split = dir.Split('/');

                        /*   if (dir_split[dir_split.Count() - 2].Contains("DC"))
                           {
                               tipo = "mensal";
                           }
                           else
                           {
                               tipo = "semanal";
                           }
                         */

                        //windows
                        //var dir = l.WorkingDirectory.Replace("/home/producao/PrevisaoPLD/", "Z:\\").Replace("/home/compass/sacompass/previsaopld/", "Z:\\").Replace("/", "\\");




                        /*   var dt_estudo = dir.Split('\\').Last().Split('/').Last();
                           var mes = Convert.ToInt32(dt_estudo.Substring(4, 2));
                           var ano = Convert.ToInt32(dt_estudo.Substring(0, 4));
                           */

                        var mes = dt_estudo.Month;
                        var ano = dt_estudo.Year;

                        var pld_lim = PLD_Limites(ano);

                        double PLD_limite = pld_lim[0];
                        double PLD_Max = pld_lim[1];

                        var dec_oper = Directory.GetFiles(dir, "dec_oper_sist.csv");
                        double cmo = 0;
                        if (dec_oper.Length > 0)
                        {
                            var infos = File.ReadAllLines(dec_oper[0]);
                            foreach (var line in infos)
                            {
                                int semana = 0;

                                var dados = line.Split(';');

                                if ((dados.Count() > 20) && (int.TryParse(dados[0].Trim(), out semana)))
                                {
                                    if ((dados[0].Trim() == dados[1].Trim()) && (dados[4].Trim() != "11"))
                                    {
                                        if (dados[3].Trim() != "-")
                                        {
                                            cmo = Convert.ToDouble(dados[23].Trim().ToString());
                                            var horas = Convert.ToDouble(dados[3].Trim().ToString());
                                            if (cmo > PLD_limite)
                                            {
                                                if (cmo > PLD_Max)
                                                {
                                                    Soma_CMO = Soma_CMO + horas * PLD_Max;
                                                }
                                                else
                                                {
                                                    Soma_CMO = Soma_CMO + horas * cmo;
                                                }
                                            }
                                            else
                                            {
                                                Soma_CMO = Soma_CMO + horas * PLD_limite;
                                            }
                                            Soma_Horas = Soma_Horas + horas;
                                        }
                                        else
                                        {
                                            cmo = Convert.ToDouble(dados[23].Trim().ToString());
                                            PLD = Soma_CMO / Soma_Horas;
                                            double Pld_Mensal = 0;
                                            int dias_Semana_Atual = 0;
                                            if (tipo == "mensal" && semana == 1)
                                            {
                                                Pld_Mensal = PLD;
                                            }
                                            else if (tipo == "mensal")
                                            {
                                                Pld_Mensal = 0;
                                            }
                                            else
                                            {
                                                int[] dias_semana = Dias_Semanas(mes, ano);
                                                try
                                                {
                                                    dias_Semana_Atual = dias_semana[Convert.ToInt32(dados[1].Trim())];
                                                }
                                                catch
                                                {

                                                }
                                                var dias_mes = DateTime.DaysInMonth(ano, mes);

                                                Pld_Mensal = (PLD * dias_Semana_Atual) / dias_mes;
                                            }
                                            object[,] Conjunto_Dados = new object[1, 7] {
                                        {
                                            semana,
                                            dados[4].Trim(),
                                            cmo,
                                            PLD,
                                            dt_estudo,
                                            tipo,
                                            Pld_Mensal

                                        }
                                    };

                                            Resu.Add(new Resu_PLD_Mensal(Conjunto_Dados));

                                            PLD = 0;
                                            Soma_CMO = 0;
                                            Soma_Horas = 0;

                                        }



                                    }
                                }


                            }

                        }
                        string json = JsonConvert.SerializeObject(Resu);

                        var arq = Path.Combine(dir, "PLD_Mensal.csv");
                        //  if (Auto)
                        if (File.Exists(arq))
                        {
                            File.Delete(arq);
                        }
                        using (TextWriter tw = new StreamWriter(arq, false, Encoding.Default))
                        {
                            tw.WriteLine(dir + "\n");
                            tw.WriteLine("Semana;Submercado;CMO;PLD;Mes;Tipo;PLD_Mensal");
                            foreach (var dado in Resu)
                            {
                                tw.WriteLine(dado.Semana + ";" + dado.Submercado + ";" + dado.CMO + ";" + dado.PLD + ";" + dado.Mes + ";" + dado.Tipo + ";" + dado.PLD_Mensal); //escreve no arquivo novamente
                            }

                            tw.Close();
                        }



                        return json;
                    }
                    else
                    {
                        Console.Write("Dadger não encontrado");
                        return null;
                    }
                }
                catch (Exception e)
                {
                    Console.Write("Erro ao Calcular PLD Mensal");
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        public static int[] Dias_Semanas(int mes, int ano) // Returna quantidade de dias para cada semana do mes OBS: Semana de Sabado a Sexta
        {
            int[] semana_dias = new int[7];
            var semana = 1;
            var dias = 1;
            var dias_mes = DateTime.DaysInMonth(ano, mes);

            for (var dia = 1; dia < dias_mes; dia++)
            {
                DateTime data = new DateTime(ano, mes, dia);

                if (data.DayOfWeek == DayOfWeek.Friday)
                {
                    semana_dias[semana] = dias;
                    dias = 0;
                    semana++;
                }
                dias++;
            }

            semana_dias[semana] = dias;

            return semana_dias;
        }

        public class Resu_PLD_Mensal
        {
            public int Semana { get; set; }
            public int Submercado { get; set; }
            public double CMO { get; set; }
            public double PLD { get; set; }

            public int Mes { get; set; }

            public string Tipo { get; set; }


            public double PLD_Mensal { get; set; }

            public Resu_PLD_Mensal(object[,] dados)
            {
                DateTime data = Convert.ToDateTime(dados[0, 4]);
                Semana = int.Parse(dados[0, 0].ToString());
                Submercado = int.Parse(dados[0, 1].ToString());
                CMO = Math.Round(Double.Parse(dados[0, 2].ToString()), 2);
                PLD = Math.Round(Double.Parse(dados[0, 3].ToString()), 2);
                // Mes = int.Parse(dados[0, 4].ToString());
                Mes = data.Month;
                Tipo = dados[0, 5].ToString();
                PLD_Mensal = Math.Round(Double.Parse(dados[0, 6].ToString()), 2);
            }
        }

        public static double[] PLD_Limites(int ano) //Carrega os Limites de PLD referente ao ANO
        {
            try
            {
                StreamReader rd = new StreamReader(@"/home/compass/sacompass/previsaopld/shared/linuxQueue/Config_PLD.csv");
                //StreamReader rd = new StreamReader(@"Z:\shared\linuxQueue\Config_PLD.csv");

                string linha = null;

                string[] dado = null;
                double[] pld = new double[2];
                while ((linha = rd.ReadLine()) != null)
                {
                    dado = linha.Split(';');

                    if (dado[0] == ano.ToString())
                    {
                        pld[0] = Convert.ToDouble(dado[1]);//PLD Minimo
                        pld[1] = Convert.ToDouble(dado[2]);//PLD Maximo
                    }

                }
                rd.Close();

                return pld;

            }
            catch
            {
                return null;

            }

        }

    }

}
