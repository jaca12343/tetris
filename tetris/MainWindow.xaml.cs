using System.DirectoryServices;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace tetris
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    internal class Fall { }
    internal class Refresh { }
    public partial class MainWindow : Window
    {
        private DispatcherTimer timer;
        Label[,] plansza = new Label[10,20];
        Label[,] holdMatrix = new Label[4, 4];
        Label[,,] futureMatrix = new Label[3, 4, 4];
        int[] futureBlocks = new int[3];
        int[] futureColors = new int[3];
        int[,] planszaint = new int[10, 20];
        int hold = -1, holdColor = 0;
        bool canHold = true;
        int[] next = new int[3];
        bool gameRunning = false;
        int ticks = 0;
        int poziom = 0;
        DateTime begin;
        

        bool[,,] klocki =
        {
            {//Klocek t
                {false,false,false,false },
                {true,true,true,false },
                {false,true,false,false },
                {false,false,false,false }
            },
            {//Klocek i
                {false,true,false,false },
                {false,true,false,false },
                {false,true,false,false },
                {false,true,false,false }
            },
            {//Klocek o
                {false,false,false,false },
                {false,true,true,false },
                {false,true,true,false },
                {false,false,false,false }
            },
            { //Klocek z
                {false,false,false,false },
                {false,true,true,false },
                {false,false,true,true },
                {false,false,false,false }
            },
            { //Klocek s
                {false,false,false,false },
                {false,true,true,false },
                {true,true,false,false },
                {false,false,false,false }
            },
            { //Klocek l
                {false,true,false,false },
                {false,true,false,false },
                {false,true,true,false },
                {false,false,false,false }
            },
            { //Klocek j
                {false,false,true,false },
                {false,false,true,false },
                {false,true,true,false },
                {false,false,false,false }
            },

        };
        SolidColorBrush[] kolory = {
            new SolidColorBrush(Color.FromArgb(255, 30, 30, 30)),
            new SolidColorBrush(Color.FromArgb(255, 30, 30, 180)),
            new SolidColorBrush(Color.FromArgb(255, 30, 150, 255)),
            new SolidColorBrush(Color.FromArgb(255, 30, 220, 30)),
            new SolidColorBrush(Color.FromArgb(255, 230, 230, 30)),
            new SolidColorBrush(Color.FromArgb(255, 230, 30, 30)),
            new SolidColorBrush(Color.FromArgb(255, 230, 130, 30)),
            new SolidColorBrush(Color.FromArgb(255, 230, 30, 230))
        };
        Random random = new Random();
        int lastBlock;
        int lastColor;
        int orientation = 0;
        int punkty;
        public MainWindow()
        {
            InitializeComponent();
            KeyDown += KeyDownHandler;
            //tworzenie glownej panszy
            for(int i = 0; i<10; i++)
            {
                for(int j = 0; j<20; j++)
                {
                    plansza[i,j]= new Label();
                    plansza[i, j].Width=35;
                    plansza[i, j].Height=35;
                    plansza[i, j].Margin=new Thickness(i*35, j*35, 315-(i*35), 665-(j*35));
                    plansza[i, j].BorderThickness=new Thickness(1,1,1,1);
                    plansza[i, j].BorderBrush=Brushes.White;
                    plansza[i, j].Background=kolory[0];
                    Main.Background=new SolidColorBrush(Color.FromArgb(255, 30, 30, 50));
                    mainGrid.Children.Add(plansza[i, j]);
                    planszaint[i,j] = 0;
                }
            }
            //tworzenie przechowywania klockow
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    holdMatrix[i, j] = new Label();
                    holdMatrix[i, j].Width = 35;
                    holdMatrix[i, j].Height = 35;
                    holdMatrix[i, j].Margin = new Thickness(i * 35, j * 35, 105 - (i * 35), 105 - (j * 35));
                    holdMatrix[i, j].BorderThickness = new Thickness(0, 0, 0, 0);
                    holdMatrix[i, j].Background =kolory[0];
                    holdGrid.Children.Add(holdMatrix[i, j]);
                }
            }
            //przyszłe bloki
            for (int m = 0; m < 3; m++)
            {
                futureBlocks[m] = random.Next()% klocki.GetLength(0);
                futureColors[m] = random.Next()% (kolory.Length - 1) + 1;
                for (int i = 0; i < 4; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        futureMatrix[m, i, j] = new Label();
                        futureMatrix[m, i, j].Width = 35;
                        futureMatrix[m, i, j].Height = 35;
                        futureMatrix[m, i, j].Margin = new Thickness(i * 35,(m*140) + (j * 35), 105 - (i * 35), 385 - (m * 140) - (j * 35) );
                        futureMatrix[m, i, j].BorderThickness = new Thickness(0, 0, 0, 0);
                        futureMatrix[m, i, j].Background = kolory[0];
                        futureGrid.Children.Add(futureMatrix[m,i, j]);
                        if (klocki[futureBlocks[m], j, i])
                        {
                            futureMatrix[m, i, j].Background = kolory[futureColors[m]];
                        }
                    }
                }
            }

        }
        public void koniecGry()
        {
            lblWynik.Visibility= Visibility.Visible;
            lblWynik.Content = "Zdobyłeś " + punkty.ToString() + " punktów";
            btn1.Visibility= Visibility.Visible;
            mainGrid.Visibility= Visibility.Collapsed;
            gameRunning = false;
            //wstawianie wyniku do pliku
            DateTime now = DateTime.Now;
            string wpis = "Punkty: "+ punkty.ToString() +", czas: "+(Math.Round((now-begin).TotalSeconds, 0)).ToString() +"s\n";
            //tu by było zapisywanie do pliku
        }
        public int[] getNextBlock()
        {
            int[] returned = new int[2];
            returned[0] = futureBlocks[0];
            returned[1] = futureColors[0];
            futureBlocks[0] = futureBlocks[1];
            futureColors[0] = futureColors[1];
            futureBlocks[1] = futureBlocks[2];
            futureColors[1] = futureColors[2];
            futureBlocks[2] = random.Next() % klocki.GetLength(0);
            futureColors[2] = random.Next() % (kolory.Length - 1) + 1;
            for (int m = 0; m < 3; m++)
            {
                for (int i = 0; i < 4; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        if (klocki[futureBlocks[m], j, i])
                        {
                            futureMatrix[m, i, j].Background = kolory[futureColors[m]];
                        }
                        else
                        {
                            futureMatrix[m, i, j].Background = kolory[0];
                        }
                    }
                }
            }
            return returned;
        }
        public void addBlock(int id, int kolor) {
            for(int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++) {
                    if (klocki[id, j, i])
                    {
                        if (planszaint[i + 4, j] == 0)
                        {
                            planszaint[i + 4, j] = -kolor;
                            plansza[i + 4, j].Background = kolory[kolor];
                            lblWynik.Content = "Punkty: " + punkty.ToString();
                        }
                        
                        else
                        {
                            koniecGry();
                            return;
                        }
                    }
                }
            }
            punkty++;
            orientation = 0;
            lastColor = kolor;
        }
        public void stopBlocks(bool genNewBlocks)
        {
            for (int y = 19; y >=0; y--)
            {
                for (int x = 9; x >=0; x--)
                {
                    if (planszaint[x, y]<0)
                    {
                        planszaint[x, y] = -planszaint[x, y];
                        plansza[x, y].Background=kolory[planszaint[x, y]];
                    }
                }
            }
            checkLines();
            if (genNewBlocks)
            {
                int[] next = getNextBlock();
                lastBlock = next[0];
                addBlock(next[0], next[1]);
                
            }
            canHold = true;
        }
        public void changeNeighbours(int x, int y)
        {
            planszaint[x, y] *= -1;
            if (x > 0)
                if (planszaint[x - 1, y] > 0)
                    changeNeighbours(x - 1, y);
            if (x < 9)
                if (planszaint[x + 1, y] > 0)
                    changeNeighbours(x + 1, y);
            if (y > 0)
                if (planszaint[x, y - 1] > 0)
                    changeNeighbours(x, y - 1);
            if (y < 19)
                if (planszaint[x, y + 1] > 0)
                    changeNeighbours(x, y + 1);
        }
        public async void checkLines() {
            int zniszczona = 0;
            int destroyedLines = 0;
            //Sprawdzamy czy mozemy usunac linie, jesli tak to usuwamy
            for (int y = 0; y < 20; y++)
            {
                bool destroyLine = true;
                for(int x = 0; x < 10; x++)
                    if(planszaint[x, y] <= 0)
                        destroyLine = false;
                if(destroyLine)
                    destroyedLines++;
            }
            if (destroyedLines == 0)
                return;
            Task.Delay(200).Wait();
            //usuwanie linii
            for (int y = 0; y < 20; y++)
            {
                bool destroyLine = true;
                for (int x = 0; x < 10; x++)
                    if (planszaint[x, y] <= 0)
                        destroyLine = false;
                if (destroyLine)
                    for (int x = 0; x < 10; x++)
                    {
                        planszaint[x, y] = 0;
                        plansza[x, y].Background = kolory[0];
                        zniszczona = y;
                    }
                
            }
            switch (destroyedLines)
            {
                case 4:
                    punkty += 600;
                    break;
                case 3:
                    punkty += 400;
                    break;
                case 2:
                    punkty += 250;
                    break;
                case 1:
                    punkty += 100;
                    break;
            }
            for (int y = zniszczona; y > 0; y--)
            {
                for (int x = 0; x < 10; x++)
                {
                    if (planszaint[x, y] > 0)
                    {
                        changeNeighbours(x, y);
                        moveBlocks(true,false);
                    }
                }
            }
        }

        public void moveBlocks(bool tillend, bool genNewBlocks)
        {
            startmove:
            for (int y = 0; y < 20; y++) {
                for (int x = 0; x < 10; x++)
                {
                    if (planszaint[x, y]<0&&y==19)
                    {
                        stopBlocks(genNewBlocks);
                        return;
                    }
                    if (planszaint[x, y]<0&&planszaint[x,y+1]>0) {
                        stopBlocks(genNewBlocks);
                        return;
                    }
                }
            }
            for (int y = 19; y >=0; y--)
            {
                for (int x = 9; x >=0; x--)
                {
                    if (planszaint[x, y]<0)
                    {
                        planszaint[x, y+1] = planszaint[x, y];
                        plansza[x, y+1].Background=kolory[-(planszaint[x, y])];

                        planszaint[x, y] = 0;
                        plansza[x, y].Background=kolory[0];
                    }
                }
            }
            if (tillend)
            {
                goto startmove;
            }
        }
        public void moveRight()
        {
            for (int y = 0; y < 20; y++)
            {
                if (planszaint[9, y]<0)
                {
                    return;
                }
                for (int x = 0; x < 10; x++)
                {
                    if (planszaint[x, y]<0&&planszaint[x+1, y]>0)
                    {
                        return;
                    }
                }
            }
            for (int y = 19; y >=0; y--)
            {
                for (int x = 9; x >=0; x--)
                {
                    if (planszaint[x, y]<0)
                    {
                        planszaint[x+1, y] = planszaint[x, y];
                        plansza[x+1, y].Background=kolory[-(planszaint[x, y])];

                        planszaint[x, y] = 0;
                        plansza[x, y].Background=kolory[0];
                    }
                }
            }
        }
        public void moveLeft()
        {
            for (int y = 0; y < 20; y++)
            {
                if (planszaint[0, y]<0)
                {
                    return;
                }
                for (int x = 0; x < 10; x++)
                {
                    if (planszaint[x, y]<0&&planszaint[x-1, y]>0)
                    {
                        return;
                    }
                }
            }
            for (int y = 19; y >=0; y--)
            {
                for (int x = 0; x <10; x++)
                {
                    if (planszaint[x, y]<0)
                    {
                        planszaint[x-1, y] = planszaint[x, y];
                        plansza[x-1, y].Background=kolory[-(planszaint[x, y])];

                        planszaint[x, y] = 0;
                        plansza[x, y].Background=kolory[0];
                    }
                }
            }
        }
        int[] checkBlock()
        {
            int[] ints = new int[2];
            ints[0] = 0;
            ints[1]=0;
            int[] returned = new int[2];
            for (int j = 0; j < 4; j++)
            {
                for (int i = 0; i < 4; i++)
                {
                    switch (orientation)
                    {
                        case (0):
                            ints[0] = i;
                            ints[1] = j;
                            break;
                        case (1):
                            ints[0] = j;
                            ints[1] = 3-i;
                            break;
                        case (2):
                            ints[0] = 3-i;
                            ints[1] = 3 - j;
                            break;
                        case (3):
                            ints[0] = 3 - j;
                            ints[1] = i;

                            break;
                    }
                    if (klocki[lastBlock, ints[1], ints[0]])
                    {
                        returned[0] = i;
                        returned[1] = j;
                        return returned;
                    }
                }
            }
            return ints;
        }
        public void rotateRight()
        {
            //tworzenie kopii planszy
            int[,] nowaPlansza = new int[14, 24];
            for (int y = 0; y < 24; y++)
                for (int x = 0; x < 14; x++)
                    nowaPlansza[x, y] = 1000;
            for (int y = 0; y < 20; y++)
                for (int x = 0; x < 10; x++)
                    nowaPlansza[x+2, y+2] = planszaint[x, y];

            
            int xinit=0,xchange=0,xmiddle=0;
            int yinit=0,ychange=0,ymiddle=0;
            int kolor = 0;
            bool pierwszy=true;
            //szukamy pierwszej liczby ujemnej
            for(int y = 2; y <22; y++)
            {
                for (int x = 2; x < 12; x++)
                {
                    if (nowaPlansza[x,y] < 0)
                    {
                        //porownujemy z blokiem
                        if (pierwszy)
                        {
                            kolor = nowaPlansza[x, y];
                            int[] temp = checkBlock();
                            xinit = x-temp[0];
                            yinit = y-temp[1];
                            xmiddle = xinit + 1;
                            ymiddle = yinit + 1;
                            pierwszy = false;
                        }
                    }
                }
            }
            orientation++;
            if(orientation == 4) {
                orientation = 0;
            }
            int[]zmieniany = new int[2];
            bool ruszylWLewo = false, ruszylWPrawo = false;
            nieUdaloSie:
            //usuwamy stary klocek
            for (int y = 2; y < 22; y++)
            {
                for (int x = 2; x < 12; x++)
                {
                    if (nowaPlansza[x, y] < 0)
                    {
                        nowaPlansza[x, y] = 0;
                    }

                }
            }
            bool wlewo = false, wprawo = false;
            for (int x = 0;x < 4; x++) {
                for (int y = 0; y < 4; y++)
                {
                    switch (orientation)
                    {
                        case (0):
                            xchange = x;
                            ychange = y;
                            break;   
                        case (1):    
                            xchange = y;
                            ychange = 3 - x;
                            break;   
                        case (2):    
                            xchange = 3 - x;
                            ychange = 3 - y;
                            break;   
                        case (3):    
                            xchange = 3 - y;
                            ychange = x;
                            break;
                    }
                    if (klocki[lastBlock, ychange, xchange]==true)
                    {
                        //jesli nachodzi na klocka lub wychodzi z planszy
                        if (nowaPlansza[xinit + x, yinit + y]>0)
                        {
                            if (xinit + x <= xmiddle )
                                wprawo = true;
                            else
                                wlewo = true;
                        }
                        else
                        {
                            nowaPlansza[xinit + x, yinit + y] = kolor;
                        }
                    }

                }
            }
            if ((wlewo && wprawo) || (ruszylWLewo && wlewo) || (ruszylWPrawo && wprawo))
            {
                orientation--;
                if (orientation == -1)
                    orientation = 3;
                return;
            }
            if (wprawo)
                xinit++;
            if (wlewo)
            {
                xinit--;
                ruszylWLewo = true;
            }
            if (wprawo || wlewo)
                goto nieUdaloSie;
            //wstawianie spowrotem do oryginalu
            for (int x = 0; x < 10; x++)
                for (int y = 0; y < 20; y++)
                {
                    planszaint[x, y] = nowaPlansza[x + 2, y + 2];
                    plansza[x, y].Background = kolory[Math.Abs(planszaint[x, y])];
                }

        }
        public void rotateLeft()
        {
            //tworzenie kopii planszy
            int[,] nowaPlansza = new int[14, 24];
            for (int y = 0; y < 24; y++)
                for (int x = 0; x < 14; x++)
                    nowaPlansza[x, y] = 1000;
            for (int y = 0; y < 20; y++)
                for (int x = 0; x < 10; x++)
                    nowaPlansza[x + 2, y + 2] = planszaint[x, y];


            int xinit = 0, xchange = 0, xmiddle = 0;
            int yinit = 0, ychange = 0, ymiddle = 0;
            int kolor = 0;
            bool pierwszy = true;
            //szukamy pierwszej liczby ujemnej
            for (int y = 2; y < 22; y++)
            {
                for (int x = 2; x < 12; x++)
                {
                    if (nowaPlansza[x, y] < 0)
                    {
                        //porownujemy z blokiem
                        if (pierwszy)
                        {
                            kolor = nowaPlansza[x, y];
                            int[] temp = checkBlock();
                            xinit = x - temp[0];
                            yinit = y - temp[1];
                            xmiddle = xinit + 1;
                            ymiddle = yinit + 1;
                            pierwszy = false;
                        }
                    }
                }
            }
            orientation--;
            if (orientation == -1)
            {
                orientation = 3;
            }
            int[] zmieniany = new int[2];
            bool ruszylWLewo = false, ruszylWPrawo = false;
        nieUdaloSie:
            //usuwamy stary klocek
            for (int y = 2; y < 22; y++)
            {
                for (int x = 2; x < 12; x++)
                {
                    if (nowaPlansza[x, y] < 0)
                    {
                        nowaPlansza[x, y] = 0;
                    }

                }
            }
            bool wlewo = false, wprawo = false;
            for (int x = 0; x < 4; x++)
            {
                for (int y = 0; y < 4; y++)
                {
                    switch (orientation)
                    {
                        case (0):
                            xchange = x;
                            ychange = y;
                            break;
                        case (1):
                            xchange = y;
                            ychange = 3 - x;
                            break;
                        case (2):
                            xchange = 3 - x;
                            ychange = 3 - y;
                            break;
                        case (3):
                            xchange = 3 - y;
                            ychange = x;
                            break;
                    }
                    if (klocki[lastBlock, ychange, xchange] == true)
                    {
                        //jesli nachodzi na klocka lub wychodzi z planszy
                        if (nowaPlansza[xinit + x, yinit + y] > 0)
                        {
                            if (xinit + x <= xmiddle)
                                wprawo = true;
                            else
                                wlewo = true;
                        }
                        else
                        {
                            nowaPlansza[xinit + x, yinit + y] = kolor;
                        }
                    }

                }
            }
            if ((wlewo && wprawo) || (ruszylWLewo && wlewo) || (ruszylWPrawo && wprawo))
            {
                orientation++;
                if (orientation == 4)
                    orientation = 0;
                return;
            }
            if (wprawo)
                xinit++;
            if (wlewo)
            {
                xinit--;
                ruszylWLewo = true;
            }
            if (wprawo || wlewo)
                goto nieUdaloSie;
            //wstawianie spowrotem do oryginalu
            for (int x = 0; x < 10; x++)
                for (int y = 0; y < 20; y++)
                {
                    planszaint[x, y] = nowaPlansza[x + 2, y + 2];
                    plansza[x, y].Background = kolory[Math.Abs(planszaint[x, y])];
                }

        }
        public void deleteNegative()
        {
            for(int x = 0;x < 10; x++)
            {
                for(int y = 0;y < 20; y++)
                {
                    if (planszaint[x, y] < 0)
                    {
                        planszaint[x, y] = 0;
                        plansza[x, y].Background = kolory[0];
                    }
                }
            }
        }
        public void holdBlock()
        {
            if (!canHold)
                return;
            canHold = false;
            int temp, tempColor;
            temp = lastBlock;
            tempColor = lastColor;
            deleteNegative();
            if (hold == -1)
            {
                int[]next = getNextBlock();
                lastBlock = next[0];
                addBlock(next[0], next[1]);
            }
            else
            {
                lastBlock = hold;
                addBlock(hold,holdColor);
            }
            hold = temp;
            holdColor = tempColor;
            for(int x = 0;x < 4; x++)
            {
                for(int y = 0;y < 4; y++)
                {
                    if (klocki[hold, y, x])
                    {
                        holdMatrix[x, y].Background = kolory[holdColor];
                    }
                    else
                    {
                        holdMatrix[x, y].Background = kolory[0];
                    }
                }
            }
        }
        private void KeyDownHandler(object sender, KeyEventArgs e) {

            if (e.Key ==Key.Down)
            {
                moveBlocks(false, true);
                punkty++;
            }
            if (e.Key ==Key.Space)
            {
                moveBlocks(true, true);
                punkty+=10;
            }
            if (e.Key ==Key.Right)
            {
                moveRight();
            }
            if (e.Key ==Key.Left)
            {
                moveLeft();
            }
            if (e.Key ==Key.Up)
            {
                rotateLeft();
            }
            if (e.Key == Key.Z)
            {
                rotateRight();
            }
            if (e.Key == Key.C)
            {
                holdBlock();
            }
        }
        private void TickFunction(object Sender, EventArgs e)
        {
            if (!gameRunning)
                return;
            int opoznienie = Math.Max(8-(poziom/300), 2);//setne sekundy
            ticks++;
            lblWynik.Content = "Punkty: "+ punkty;
            lblPoziom.Content = "Poziom: "+ poziom;
            DateTime now = DateTime.Now;
            if (punkty >poziom*100)
                poziom++;
            lblCzas.Content = "Czas: " + (Math.Round((now-begin).TotalSeconds,1)).ToString();
            if (ticks % opoznienie == 0)
            {
                moveBlocks(false, true);
            }
            

        }
        private async void startGame(object sender, RoutedEventArgs e)
        {
            mainGrid.Visibility = Visibility.Visible;
            btn1.Visibility = Visibility.Collapsed;
            punkty = 1;
            int kolor = (random.Next() % (kolory.Length - 1)) + 1;
            lastBlock = random.Next() % klocki.GetLength(0);
            orientation = 0;
            holdGrid.Visibility = Visibility.Visible;
            poziom = 0;

            addBlock(lastBlock, kolor);
            gameRunning = true;
            
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(100);
            timer.Tick += new EventHandler(TickFunction);
            begin = DateTime.Now;
            timer.Start();
        }

    }
}