using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace FileSearch
{
    public partial class MainWindow : Window
    {
        public ObservableCollection<SearchResult> Results { get; set; } = new ObservableCollection<SearchResult>();

        public MainWindow()
        {
            InitializeComponent();
            ResultListView.ItemsSource = Results;
        }

        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string word = WordTextBox.Text ?? "";
            string directoryPath = DirectoryTextBox.Text ?? "";

            if (!Directory.Exists(directoryPath))
            {
                MessageBox.Show("Directory not found.");
                return;
            }
            await SearchFiles(directoryPath, word);
        }

        private async Task SearchFiles(string directoryPath, string word)
        {
            Results.Clear();

            string[] files = Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories);
            int totalWords = 0;
            int wordOccurrences = 0;

            foreach (string file in files)
            {
                await Task.Run(() =>
                {
                    try
                    {
                        CountWordOccurrences(file, word, out int wordCount, out int totalWordsInFile);
                        totalWords += totalWordsInFile;
                        wordOccurrences += wordCount;
                        if (wordCount > 0)
                        {
                            SearchResult result = new SearchResult
                            {
                                FileName = Path.GetFileName(file),
                                FilePath = file,
                                WordOccurrences = wordCount,
                                TotalWordsInFile = totalWordsInFile,
                                PercentageOfOccurrences = (double)wordCount / totalWordsInFile * 100
                            };
                            Application.Current.Dispatcher.Invoke(() => Results.Add(result));
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error processing file {file}: {ex.Message}");
                    }
                });
            }

            MessageBox.Show($"Total words: {totalWords}\nTotal occurrences of the word: {wordOccurrences}\nPercentage of occurrences: {(double)wordOccurrences / totalWords * 100}%");
        }

        private void CountWordOccurrences(string filePath, string word, out int wordCount, out int totalWords)
        {
            wordCount = 0;
            totalWords = 0;
            using (StreamReader reader = new StreamReader(filePath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    string[] words = line.Split(' ');
                    totalWords += words.Length;
                    foreach (string w in words)
                    {
                        if (w.Equals(word, StringComparison.OrdinalIgnoreCase))
                        {
                            wordCount++;
                        }
                    }
                }
            }
        }
    }

    public class SearchResult
    {
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public int WordOccurrences { get; set; }
        public int TotalWordsInFile { get; set; }
        public double PercentageOfOccurrences { get; set; }
    }
}
