using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup.Localizer;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml.Serialization;
using Microsoft.Win32;

namespace Labirynth
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static List<List<Square>> lsts = new List<List<Square>>();   //lista kwardratów z których zbudowana jest plansza
        public static int rows = 6;                 
        public static int columns = 6;
        public static int startX = -1;
        public static int startY = -1;
        public static int endX = -1;
        public static int endY = -1;
        public static bool paused;
        public static bool ended;

        public MainWindow()
        {
            InitializeComponent();
        }

        //funkcja obsługująca kliknięcie na kwardrat, jeżeli może to zmienia z ściany na wolne pole i odwrotnie
        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            foreach (var list in lsts)
            {
                var rect = (Rectangle) sender;
                var sqr = list.Find(x => x.Tag == ((Square)rect.DataContext).Tag);
                if (sqr != null)
                {
                    if (sqr.Ending || sqr.Starting) return;
                    if (sqr.Wall)
                    {
                        sqr.Filling.Color = Colors.CadetBlue;
                        sqr.Wall = false;
                    }
                    else
                    {
                        sqr.Filling.Color = Colors.Black;
                        sqr.Wall = true;
                    }
                }
            }
            InitializeComponent();

        }

        //funkcja tworząca nowy labirynt
        private void BtCreate_OnClick(object sender, RoutedEventArgs e)
        {
            CreateWindow crt = new CreateWindow();
            crt.ShowDialog();
            lsts = new List<List<Square>>();
            for (int i = 0; i < rows; i++)
            {
                lsts.Add(new List<Square>());

                for (int j = 0; j < columns; j++)
                {
                    lsts[i].Add(new Square(i + "," + j));
                }
            }
            lst.ItemsSource = lsts;
            InitializeComponent();

            makeStart(0, 0);
            makeEnd(rows - 1, columns - 1);
        }

        //funkcja do wskazywania nowego punktu startu
        private void BtStartPoint_OnClick(object sender, RoutedEventArgs e)
        {
            foreach (var list in lsts)
            {
                var sqr = list.Find(x => x.Starting == true);
                if (sqr != null)
                {
                    sqr.Starting = false;
                    sqr.Filling.Color = Colors.CadetBlue;
                }
            }
            StartWindow crt = new StartWindow();
            crt.ShowDialog();
            foreach (var list in lsts)
            {
                var sqr = list.Find(s => s.Tag == startX + "," + startY);
                if (sqr != null)
                {
                    if (sqr.Wall) sqr.Wall = false;
                    sqr.Starting = true;
                    sqr.Filling.Color = Colors.Yellow;
                }
            }
        }

        //pomocnicza funkcja do zamiany normalnego pola w pole start
        public static void makeStart(int x, int y)
        {
            foreach (var list in lsts)
            {
                var sqr = list.Find(s => s.Tag == x + "," + y);
                if (sqr != null)
                {
                    if (sqr.Wall) sqr.Wall = false;
                    sqr.Starting = true;
                    sqr.Filling.Color = Colors.Yellow;
                    startX = x;
                    startY = y;
                }
            }
        }

        //funkcja do wskazywania nowego punktu zakończenia
        private void BtEndPoint_OnClick(object sender, RoutedEventArgs e)
        {
            foreach (var list in lsts)
            {
                var sqr = list.Find(x => x.Ending == true);
                if (sqr != null)
                {
                    sqr.Ending = false;
                    sqr.Filling.Color = Colors.CadetBlue;
                }
            }
            EndWindow crt = new EndWindow();
            crt.ShowDialog();
            makeEnd(endX, endY);
        }

        //pomocnicza funkcja zamieniająca normalne pole w pole końcowe
        public static void makeEnd(int x, int y)
        {
            foreach (var list in lsts)
            {
                var sqr = list.Find(s => s.Tag == x + "," + y);
                if (sqr != null)
                {
                    if (sqr.Wall) sqr.Wall = false;
                    sqr.Ending = true;
                    sqr.Filling.Color = Colors.Green;
                    endX = x;
                    endY = y;
                }
            }
        }

        //funkcja zapisująca graf do pliku
        private void BtSave_OnClick(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                FileName = "graph.txt",
                Filter = "Graph files (*.txt) | *.txt",
                DefaultExt = ".txt"
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                using (var writer = new StreamWriter(saveFileDialog.FileName))
                {
                    writer.WriteLine(rows + " " + columns);
                    for (int i = 0; i < rows; i++)
                    {
                        var line = "";
                        for (int j = 0; j < columns; j++)
                        {
                            Square sqr = null;
                            foreach (var list in lsts)
                            {
                                sqr = list.Find(x => x.Tag == i + "," + j);
                                if (sqr != null) break;
                            }
                            if (sqr != null && sqr.Wall)
                            {
                                line += "1 ";
                            }
                            else
                            {
                                line += "0 ";
                            }
                        }
                        line = line.Remove(line.Length - 1, 1);
                        writer.WriteLine(line);
                    }
                    writer.WriteLine(startX + " " + startY);
                    writer.WriteLine(endX + " " + endY);
                }
            }
        }

        //funkcja otwierająca graf z pliku tekstowego
        private void BtLoad_OnClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Graph file (*.txt) | *.txt"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                FileStream fs = new FileStream(openFileDialog.FileName, FileMode.Open);
                using (StreamReader sr = new StreamReader(fs))
                {
                    //read map size
                    string line = sr.ReadLine();
                    string[] words = line.Split();
                    rows = Convert.ToInt32(words[0]);
                    columns = Convert.ToInt32(words[1]);
                    //initialize empty map
                    lsts = new List<List<Square>>();
                    for (int i = 0; i < rows; i++)
                    {
                        lsts.Add(new List<Square>());

                        for (int j = 0; j < columns; j++)
                        {
                            lsts[i].Add(new Square(i + "," + j));
                        }
                    }
                    //read walls
                    for (int i = 0; i < rows; i++)
                    {
                        line = sr.ReadLine();
                        if (line != null) words = line.Split();
                        for (int j = 0; j < columns; j++)
                        {
                            if (Convert.ToInt32(words[j]) == 1)
                            {
                                lsts[i][j] = new Square(true, i + "," + j);
                            }
                        }
                    }
                    //read start
                    line = sr.ReadLine();
                    if (line != null) words = line.Split();
                    foreach (var list in lsts)
                    {
                        var sqr = list.Find(x => x.Tag == words[0] + "," + words[1]);
                        if (sqr != null)
                        {
                            if (sqr.Wall) sqr.Wall = false;
                            sqr.Starting = true;
                            sqr.Filling.Color = Colors.Yellow;
                        }
                    }
                    startX = Convert.ToInt32(words[0]);
                    startY = Convert.ToInt32(words[1]);
                    //read end
                    line = sr.ReadLine();
                    if (line != null) words = line.Split();
                    foreach (var list in lsts)
                    {
                        var sqr = list.Find(x => x.Tag == words[0] + "," + words[1]);
                        if (sqr != null)
                        {
                            if (sqr.Wall) sqr.Wall = false;
                            sqr.Ending = true;
                            sqr.Filling.Color = Colors.Green;
                        }
                    }
                    endX = Convert.ToInt32(words[0]);
                    endY = Convert.ToInt32(words[1]);
                }
            }
            lst.ItemsSource = lsts;
            InitializeComponent();
        }

        //funkcja uruchamiająca alogrytm DFS
        private void BtStartDfs_OnClick(object sender, RoutedEventArgs e)
        {
            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                map = new List<List<string>>();
                Dfs(startX, startY);
            }).Start();
            BtStartDfs.IsEnabled = false;
            BtStartBfs.IsEnabled = false;
            _done = false;
        }

        //funckja do wyszukiwania kwardratu o podanym x i y 
        private Square FindNode(int x, int y)
        {
            foreach (var list in lsts)
            {
                var sqr = list.Find(s => s.Tag == x + "," + y);
                if (sqr != null)
                {
                    return sqr;
                }
            }
            return null;
        }

        //pomocnicza funkcja do zmiany koloru kwardratu
        private void changeColor(Square sqr)
        {
            if (sqr.Starting || sqr.Ending) return;
            sqr.Filling.Color = Colors.DarkCyan;
        }

        private bool _done;
        private List<List<String>> map = new List<List<string>>();

        //algortym DFS
        private void Dfs(int x, int y)
        {
            Outer:
            if (paused) goto Outer;
            double delay = 200.0;
            string current = x + "," + y;
            Application.Current.Dispatcher.Invoke(new Action(() => { delay = SlSpeed.Value; }));
            Thread.Sleep(Convert.ToInt32(delay));
            if (_done) return;

            //wszukaj kwardrat o aktualnie sprawdzanych koordynatach
            var end = FindNode(x, y);
            //jeżeli znaleziony kwadrat ma ustawioną flagę Ending na true to poinformuj o zakończeniu
            if (end.Ending)
            {
                MessageBox.Show("Odnaleziono koniec", "Koniec", MessageBoxButton.OK, MessageBoxImage.Information);
                Application.Current.Dispatcher.Invoke(new Action(() => { _done = true; }));
                ended = true;
                HighlightPath(end.Tag);
                return;
            }
            //wyszukaj kwardrat który znajduje się z lewej strony
            var left = FindNode(x - 1, y);
            //jeżeli istnieje taki kwadrat, nie jest on ścianą i nie był dotychczas sprawdzany to...
            if (left != null && left.Wall != true && left.Checked != true)
            {
                if (_done) return;
                //zapisz połączenie poprzedni->następny między wierzchołkami
                map.Add(new List<string>());
                map[map.Count - 1].Add(current);
                map[map.Count - 1].Add(left.Tag);
                //zaznacz że był sprawdzony i zmień jego kolor
                left.Checked = true;
                Application.Current.Dispatcher.Invoke(new Action(() => { changeColor(left); }));
                //wywołaj funckję rekurencyjnie dla nowo znalezionego wierzchołka
                Dfs(x - 1, y);
            }
            //reszta działa tak samo tylko dla prawo, góra, dół
            var up = FindNode(x, y + 1);
            if (up != null && up.Wall != true && up.Checked != true)
            {
                if (_done) return;
                map.Add(new List<string>());
                map[map.Count - 1].Add(current);
                map[map.Count - 1].Add(up.Tag);
                up.Checked = true;
                Application.Current.Dispatcher.Invoke(new Action(() => { changeColor(up); }));
                Dfs(x, y + 1);
            }
            var right = FindNode(x + 1, y);
            if (right != null && right.Wall != true && right.Checked != true)
            {
                if (_done) return;
                map.Add(new List<string>());
                map[map.Count - 1].Add(current);
                map[map.Count - 1].Add(right.Tag);
                right.Checked = true;
                Application.Current.Dispatcher.Invoke(new Action(() => { changeColor(right); }));
                Dfs(x + 1, y);
            }
            var down = FindNode(x, y - 1);
            if (down != null && down.Wall != true && down.Checked != true)
            {
                if (_done) return;
                map.Add(new List<string>());
                map[map.Count - 1].Add(current);
                map[map.Count - 1].Add(down.Tag);
                down.Checked = true;
                Application.Current.Dispatcher.Invoke(new Action(() => { changeColor(down); }));
                Dfs(x, y - 1);
            }
        }

        //funkcja wyróżniająca znalezioną ścieżkę po zakończeniu algorytmu
        void HighlightPath(string dest)
        {
            if (dest == startX + "," + startY) return;
            var lst = map.Find(x => x[1] == dest);
            foreach (var list in lsts)
            {
                var sqr = list.Find(s => s.Tag == lst[0]);
                if (sqr != null)
                {
                    if (sqr.Wall && !sqr.Checked) return;
                    if (!sqr.Starting)
                    {
                        Application.Current.Dispatcher.Invoke(new Action(() => { sqr.Filling.Color = Colors.Cyan; }));
                    }
                    else
                    {
                        return;
                    }
                    Thread.Sleep(10);
                }
            }
            HighlightPath(lst[0]);
        }

        //Algortm BFS
        private void Bfs(int x, int y)
        {
            map = new List<List<string>>();
            List<Square> sqrsQ = new List<Square>();
            //dodawawnie początkowego wierzchołka
            var sqr = FindNode(x, y);
            sqr.Checked = true;
            sqrsQ.Add(sqr);
            //dopóki kolejka nie jest pusta
            while (sqrsQ.Count != 0)
            {
                double delay = 200.0;
                Application.Current.Dispatcher.Invoke(new Action(() => { delay = SlSpeed.Value; }));

                if (paused) continue;

                //jeżeli sprawdzany kwardrat ma flagę Ending ustawioną na true to poinformuj o zakończeniu
                if (sqrsQ[0].Ending)
                {
                    MessageBox.Show("Odnaleziono koniec", "Koniec", MessageBoxButton.OK, MessageBoxImage.Information);
                    ended = true;
                    HighlightPath(sqrsQ[0].Tag);
                    return;
                }

                string current;
                
                //zczytaj koordynaty aktualnego wierzchołka
                var str = sqrsQ[0].Tag;
                x = Convert.ToInt32(str.Split(',')[0]);
                y = Convert.ToInt32(str.Split(',')[1]);

                //po czym usuń go z kolejki, ponieważ został już sprawdzony
                sqrsQ.RemoveAt(0);

                //wyszukaj kwadrat na prawo od aktualnego
                var right = FindNode(x + 1, y);
                //jeżeli istnieje taki kwadrat, nie jest on ścianą i nie był dotychczas sprawdzany to...
                if (right != null && right.Wall != true && right.Checked != true)
                {
                    //zapisz połączenie poprzedni->następny między wierzchołkami
                    current = right.Tag;
                    map.Add(new List<string>());
                    map[map.Count - 1].Add(str);
                    map[map.Count - 1].Add(current);
                    Thread.Sleep(Convert.ToInt32(delay)/4);
                    //dodaj znaleziony kwardrat na koniec kolejki
                    sqrsQ.Add(right);
                    right.Checked = true;
                    Application.Current.Dispatcher.Invoke(new Action(() => { changeColor(right); }));
                }
                //reszta działa tak samo tylko dla lewo, góra, dół
                var left = FindNode(x - 1, y);
                if (left != null && left.Wall != true && left.Checked != true)
                {
                    current = left.Tag;
                    map.Add(new List<string>());
                    map[map.Count - 1].Add(str);
                    map[map.Count - 1].Add(current);
                    Thread.Sleep(Convert.ToInt32(delay) / 4);
                    sqrsQ.Add(left);
                    left.Checked = true;
                    Application.Current.Dispatcher.Invoke(new Action(() => { changeColor(left); }));
                }
                var up = FindNode(x, y + 1);
                if (up != null && up.Wall != true && up.Checked != true)
                {
                    current = up.Tag;
                    map.Add(new List<string>());
                    map[map.Count - 1].Add(str);
                    map[map.Count - 1].Add(current);
                    Thread.Sleep(Convert.ToInt32(delay) / 4);
                    sqrsQ.Add(up);
                    up.Checked = true;
                    Application.Current.Dispatcher.Invoke(new Action(() => { changeColor(up); }));
                }
                var down = FindNode(x, y - 1);
                if (down != null && down.Wall != true && down.Checked != true)
                {
                    current = down.Tag;
                    map.Add(new List<string>());
                    map[map.Count - 1].Add(str);
                    map[map.Count - 1].Add(current);
                    Thread.Sleep(Convert.ToInt32(delay) / 4);
                    sqrsQ.Add(down);
                    down.Checked = true;
                    Application.Current.Dispatcher.Invoke(new Action(() => { changeColor(down); }));
                }
            }
        }

        //funkcja uruchamiająca alorytm BFS
        private void BtStartBfs_OnClick(object sender, RoutedEventArgs e)
        {
            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                Bfs(startX, startY);
            }).Start();
            BtStartBfs.IsEnabled = false;
            BtStartDfs.IsEnabled = false;
            _done = false;
        }

        //funckja do zatrzymywania algorytmu
        private void BtPause_OnClick(object sender, RoutedEventArgs e)
        {
            if (paused)
            {
                paused = false;
                BtPause.Content = "Pause";
            }
            else
            {
                paused = true;
                BtPause.Content = "UnPause";
            }
        }

        //funkcja do resetowania algorytmu i czyszczenia planszy
        private void BtReset_OnClick(object sender, RoutedEventArgs e)
        {
            if (!ended)
            {
                MessageBox.Show("Nie można teraz zresetować", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            foreach (var list in lsts)
            {
                foreach (var sqr in list)
                {
                    if (sqr.Starting || sqr.Ending) sqr.Checked = false;
                    if (sqr.Wall != true && sqr.Starting != true && sqr.Ending != true)
                    {
                        sqr.Checked = false;
                        sqr.Filling.Color = Colors.CadetBlue;
                    }
                }
            }
            BtStartDfs.IsEnabled = true;
            BtStartBfs.IsEnabled = true;
            ended = false;
        }
    }
}
