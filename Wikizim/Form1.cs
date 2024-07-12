using System.Diagnostics;
using System.Xml;
using HtmlAgilityPack;

namespace Wikizim
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private async void ButtonNext_Click(object sender, EventArgs e)
        {
            buttonNext.Enabled = false;

            // Choix du fichier .zim
            OpenFileDialog openFileDialog = new()
            {
                Filter = "ZIM files (*.zim)|*.zim|All files (*.*)|*.*"
            };
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                textBox.Text = openFileDialog.FileName;
            }

            // Création des variables du nom, chemin d'accès et dossier d'export.
            string zimFilePath = textBox.Text;
            string zimName = Path.GetFileNameWithoutExtension(zimFilePath);
            string outputFolder;

            // Vérifie si le fichier existe, si oui execute notre code.
            if (string.IsNullOrEmpty(zimFilePath))
            {
                MessageBox.Show("Veuillez sélectionner un fichier ZIM.");
                return;
            }
            else
            {
                // Choix du dossier d'export
                FolderBrowserDialog folderBrowserDialog = new();
                if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                {
                    // Création du chemin d'accès
                    outputFolder = folderBrowserDialog.SelectedPath;

                    // Lance la fonction ExtractZimFileAsync qui lance un serveur Kiwix et export les pages du Wiki en format .html
                    await Fonctions.ExtractZimFileAsync(zimFilePath, zimName, outputFolder);

                    // Récupère les fichier .HTML extrait puis les convertis en .XML pour MediaWiki Database selon ses normes.
                    Fonctions.ConvertHtmlToXml(outputFolder);
                }
            }

            // Réactive le boutton pour permettre une nouvelle conversion
            buttonNext.Enabled = true;
        }
    }

    public class Fonctions
    {
        public static async Task ExtractZimFileAsync(string zimFilePath, string zimName, string outputFolder)
        {
            if (!Directory.Exists(outputFolder))
            {
                Directory.CreateDirectory(outputFolder);
            }

            // Construction du chemin d'accès du serveur kiwix-serve
            string kiwixServePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ExtBin", "kiwix-serve.exe");

            // Vérifiez si le fichier kiwix-serve.exe existe
            if (!File.Exists(kiwixServePath))
            {
                MessageBox.Show($"Le fichier kiwix-serve n'a pas été trouvé au chemin spécifié : {kiwixServePath}");
                return;
            }

            // Instructions de lancement pour le serveur Kiwix
            var processInfo = new ProcessStartInfo
            {
                FileName = kiwixServePath,
                Arguments = "--port=8080 " + zimFilePath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var process = Process.Start(processInfo))
            {
                // Attendre quelques secondes pour que le serveur démarre
                await Task.Delay(5000);

                // Utilisation de HttpClient pour interagir avec le serveur
                using (HttpClient client = new())
                {
                    // Ouverture de la page d'accueil du serveur Kiwix.
                    HttpResponseMessage response = await client.GetAsync($"http://localhost:8080/{zimName}");
                    response.EnsureSuccessStatusCode();

                    string content = await response.Content.ReadAsStringAsync();

                    // Exporte la page actuel en .HTML
                    File.WriteAllText(Path.Combine(outputFolder, "articles.html"), content);

                    MessageBox.Show("Extraction réussie !");
                }

                // Kill le process du serveur LocalHost (id: kiwix-serve.exe)
                process.Kill();
            }
        }

        public static void ConvertHtmlToXml(string outputFolder)
        {
            string htmlFilePath = Path.Combine(outputFolder, "articles.html");

            // Vérifier si le fichier HTML existe
            if (!File.Exists(htmlFilePath))
            {
                MessageBox.Show($"Le fichier HTML {htmlFilePath} n'existe pas.");
                return;
            }

            try
            {
                // Lire le contenu du fichier HTML
                string htmlContent = File.ReadAllText(htmlFilePath);

                // Utiliser HtmlAgilityPack pour manipuler le HTML
                HtmlAgilityPack.HtmlDocument htmlDoc = new();
                htmlDoc.LoadHtml(htmlContent);

                // Extraire les articles et créer des pages MediaWiki
                var pages = new List<(string Title, string Content)>();

                // Ici nous assumons que chaque article est contenu dans un <div> avec un identifiant spécifique
                var articleNodes = htmlDoc.DocumentNode.SelectNodes("//div[@id='bodyContent']");
                if (articleNodes != null)
                {
                    foreach (var articleNode in articleNodes)
                    {
                        var titleNode = articleNode.SelectSingleNode(".//h1");
                        var contentNode = articleNode.SelectSingleNode(".//div[@id='content']");

                        if (titleNode != null && contentNode != null)
                        {
                            string title = titleNode.InnerText.Trim();
                            string content = contentNode.InnerHtml.Trim();

                            pages.Add((title, content));
                        }
                    }
                }

                CreateMediaWikiXml(pages, Path.Combine(outputFolder, "mediawiki_dump.xml"));

                MessageBox.Show("Conversion HTML vers XML pour MediaWiki terminée !");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Une erreur est survenue : {ex.Message}");
            }
        }

        private static void CreateMediaWikiXml(List<(string Title, string Content)> pages, string outputFile)
        {
            var settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "\t",
                NewLineOnAttributes = false
            };

            using (var writer = XmlWriter.Create(outputFile, settings))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("mediawiki", "http://www.mediawiki.org/xml/export-0.10/");
                writer.WriteAttributeString("xmlns", "xsi", null, "http://www.w3.org/2001/XMLSchema-instance");
                writer.WriteAttributeString("xsi", "schemaLocation", null, "http://www.mediawiki.org/xml/export-0.10/ http://www.mediawiki.org/xml/export-0.10.xsd");
                writer.WriteAttributeString("version", "0.10");
                writer.WriteAttributeString("xml", "lang", null, "en");

                writer.WriteStartElement("siteinfo");
                writer.WriteElementString("sitename", "My Wiki");
                writer.WriteElementString("dbname", "my_wiki");
                writer.WriteElementString("base", "http://mywiki.example/Main_Page");
                writer.WriteElementString("generator", "MediaWiki 1.35.0");
                writer.WriteElementString("case", "first-letter");

                writer.WriteStartElement("namespaces");
                WriteNamespace(writer, -2, "Media");
                WriteNamespace(writer, -1, "Special");
                WriteNamespace(writer, 0, null);
                WriteNamespace(writer, 1, "Talk");
                WriteNamespace(writer, 2, "User");
                // Add more namespaces as needed
                writer.WriteEndElement(); // namespaces

                writer.WriteEndElement(); // siteinfo

                int id = 1;
                foreach (var page in pages)
                {
                    writer.WriteStartElement("page");
                    writer.WriteElementString("title", page.Title);
                    writer.WriteElementString("ns", "0");
                    writer.WriteElementString("id", id.ToString());

                    writer.WriteStartElement("revision");
                    writer.WriteElementString("id", id.ToString());
                    writer.WriteElementString("parentid", "0");
                    writer.WriteElementString("timestamp", DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"));

                    writer.WriteStartElement("contributor");
                    writer.WriteElementString("username", "Admin");
                    writer.WriteElementString("id", "1");
                    writer.WriteEndElement(); // contributor

                    writer.WriteElementString("comment", "Initial import");
                    writer.WriteElementString("model", "wikitext");
                    writer.WriteElementString("format", "text/x-wiki");

                    writer.WriteStartElement("text");
                    writer.WriteAttributeString("xml", "space", null, "preserve");
                    writer.WriteString(page.Content);
                    writer.WriteEndElement(); // text

                    writer.WriteElementString("sha1", "abcd1234"); // Dummy SHA1, replace with actual calculation if needed

                    writer.WriteEndElement(); // revision

                    writer.WriteEndElement(); // page

                    id++;
                }

                writer.WriteEndElement(); // mediawiki
                writer.WriteEndDocument();
            }
        }

        private static void WriteNamespace(XmlWriter writer, int key, string name)
        {
            writer.WriteStartElement("namespace");
            writer.WriteAttributeString("key", key.ToString());
            writer.WriteAttributeString("case", "first-letter");
            if (name != null)
            {
                writer.WriteString(name);
            }
            writer.WriteEndElement();
        }
    }
}
