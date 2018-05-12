using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Aiv.Draw;

namespace Lezione_28__24Novembre2017_Memory_vs_CPU
{
    class Program
    {
        struct Vector2
        {
            public int X;
            public int Y;
        }
        struct Color
        {
            public byte R;
            public byte G;
            public byte B;
        }
        struct Card
        {
            public Vector2 Position;
            public int Value;
            public Color ColorValue;
            public bool Revealed;
        }
        struct Deck
        {
            public Card[] Cards;
            public Vector2 Position;
            public int NumCol;
            public int CardHeight;
            public int CardWidth;
        }

        static void CreateColors(ref Color[] colors, Random r)
        {
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i].R = (byte)r.Next(10, 256);
                colors[i].G = (byte)r.Next(10, 256);
                colors[i].B = (byte)r.Next(10, 256);
            }
        }
        static void InitDeck(out Deck deck, ref Color[] color, int value)
        {
            deck.NumCol = 4;
            deck.CardWidth = 60;
            deck.CardHeight = 100;
            deck.Position.X = 10;
            deck.Position.Y = 10;

            deck.Cards = new Card[value * 2];

            for (int i = 0; i < value; i++)
            {
                deck.Cards[i].Value = i;
                deck.Cards[i + value].Value = i;
                deck.Cards[i].ColorValue = color[i];
                deck.Cards[i + value].ColorValue = color[i];
            }
            for (int i = 0; i < deck.Cards.Length; i++)
            {
                deck.Cards[i].Revealed = false;
            }

            /* for(int i = 0; i < d.Card.Length+; i++)
             * {
             * int offset = 10;
             * Vector2 nextPos;
             * nextPos.X = deck.Position.X + (i % numCol)* (cardWidth + offset);
             * nextPos.Y = deck.Position.Y + (i / numCol)*(cardHeight + offset);
             * }*/
        }
        static void InitCardPosition(ref Deck deck)
        {
            int xOff = 5;
            int yOff = 8;
            int currRowX = deck.Position.X;
            int currRowY = deck.Position.Y;
            for (int i = 0; i < deck.Cards.Length; i++)
            {
                if (i != 0 && i % deck.NumCol == 0)
                {
                    currRowY += deck.CardHeight + yOff;
                }
                deck.Cards[i].Position.X = deck.Position.X + (i % deck.NumCol) * (deck.CardWidth + xOff);
                deck.Cards[i].Position.Y = currRowY;
            }
        }

        static void ClearScreen(Window window)
        {
            for (int i = 0; i < window.bitmap.Length; i++)
            {
                window.bitmap[i] = 0;
            }
        }

        static void Shuffle(ref Deck deck, Random r)
        {
            for (int i = 0; i < deck.Cards.Length; i++)
            {
                int randomIndexA = r.Next(deck.Cards.Length);
                int randomIndexB = r.Next(deck.Cards.Length);
                Swap(ref deck.Cards[randomIndexA], ref deck.Cards[randomIndexB]);
            }
        }
        static void Swap(ref Card a, ref Card b)
        {
            Color tempColor = a.ColorValue;
            int tempValue = a.Value;

            a.ColorValue = b.ColorValue;
            a.Value = b.Value;

            b.ColorValue = tempColor;
            b.Value = tempValue;
        }

        static void PutPixel(Window window, int x, int y, byte r, byte g, byte b)
        {
            if (x < 0 || x > window.width)
                return;
            if (y < 0 || y > window.height)
                return;
            int position = (window.width * y + x) * 3;
            window.bitmap[position] = r;
            window.bitmap[position + 1] = g;
            window.bitmap[position + 2] = b;
        }
        static void DrawFullSquare(Window window, int startX, int startY, int finishX, int finishY, byte r, byte g, byte b)
        {
            for (int i = startX; i <= finishX; i++)
            {
                for (int j = startY; j <= finishY; j++)
                {
                    PutPixel(window, i, j, r, g, b);
                }
            }
        }
        static void DrawDeck(Window window, Deck deck)
        {
            for (int i = 0; i < deck.Cards.Length; i++)
            {
                DrawCard(window, deck.Cards[i], deck.CardWidth, deck.CardHeight);
            }
        }
        static void DrawCard(Window window, Card c, int width, int height)
        {
            if (c.Revealed)
            {
                DrawFullSquare(window, c.Position.X, c.Position.Y, c.Position.X + width, c.Position.Y + height, c.ColorValue.R, c.ColorValue.G, c.ColorValue.B);
            }
            else
            {
                DrawFullSquare(window, c.Position.X, c.Position.Y, c.Position.X + width, c.Position.Y + height, 150, 150, 150);
            }
        }

        static int GetSelectedCard(Window window, Deck deck)
        {
            if (window.mouseLeft)
            {
                Vector2 mousePosition;
                mousePosition.X = window.mouseX;
                mousePosition.Y = window.mouseY;
                for (int i = 0; i < deck.Cards.Length; i++)
                {
                    if (Contains(deck.Cards[i].Position, deck.CardWidth, deck.CardHeight, mousePosition))
                    {
                        return i;
                    }
                }
            }
            return -1;
        }
        static bool Contains(Vector2 position, int width, int height, Vector2 point)
        {
            return (point.X >= position.X && point.X <= position.X + width) && (point.Y >= position.Y && point.Y <= position.Y + height);
        }

        static void Main(string[] args)
        {
            Window window = new Window(2400, 2400, "Game", PixelFormat.RGB);
            Random random = new Random();

            window.opened = false;
            const float DEFAULT_CLICK_COUNTDOWN = 0.2f;
            float clickCountdown = 0;

            int value = 3;
            int revealed = value;
            Deck deck;

            int scoreG1 = 0;
            int scoreCPU = 0;
            bool playerTurn = true;

            int[] selectedCards = new int[2];
            int numberSelectedCard = 0;

            Color[] colors = new Color[value * 2];
            CreateColors(ref colors, random);
            InitDeck(out deck, ref colors, value);
            Shuffle(ref deck, random);
            InitCardPosition(ref deck);

            while (revealed > 0)
            {
                if (playerTurn)
                {
                    if (window.GetKey(KeyCode.Esc))
                        break;

                    clickCountdown -= window.deltaTime;
                    if (numberSelectedCard < 2)
                    {
                        if (clickCountdown <= 0)
                        {
                            int currentCard = GetSelectedCard(window, deck);
                            if (currentCard != -1 && !deck.Cards[currentCard].Revealed)
                            {
                                clickCountdown = DEFAULT_CLICK_COUNTDOWN;
                                deck.Cards[currentCard].Revealed = true;
                                selectedCards[numberSelectedCard++] = currentCard;
                            }
                        }
                    }
                }
                else
                {
                    if (numberSelectedCard < 2)
                    {
                        int randomCard = random.Next(deck.Cards.Length - 1);
                        if (!deck.Cards[randomCard].Revealed)
                        {
                            deck.Cards[randomCard].Revealed = true;
                            selectedCards[numberSelectedCard++] = randomCard;
                            Thread.Sleep(1000);
                        }
                    }
                }

                ClearScreen(window);
                DrawDeck(window, deck);

                window.Blit();

                if (numberSelectedCard == 2)
                {
                    if (deck.Cards[selectedCards[0]].Value == deck.Cards[selectedCards[1]].Value)
                    {
                        revealed--;
                        if (playerTurn)
                            scoreG1++;
                        if (!playerTurn)
                            scoreCPU++;
                    }
                    else
                    {
                        deck.Cards[selectedCards[0]].Revealed = false;
                        deck.Cards[selectedCards[1]].Revealed = false;
                        Thread.Sleep(1000);
                    }
                    numberSelectedCard = 0;
                    playerTurn = !playerTurn;
                }

            }
            if (scoreG1 > scoreCPU)
                Console.WriteLine(" You Win" + scoreG1 + "/" + scoreCPU);
            else
                Console.WriteLine(" CPU Win" + scoreG1 + "/" + scoreCPU);
        }

    }
}
