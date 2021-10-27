using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace MinivillesURSR46
{
    public class Screen
    {
        public int height;
        public int width;

        //Dictionary<Coordinates, string[]> elements = new();
        Dictionary<int, List<Element>> layers = new Dictionary<int, List<Element>>();
        List<Element> elementsClone = new List<Element>();

        /// <summary>
        /// Constructeur de la classe, crée une fenêtre carré
        /// </summary>
        /// <param name="size">Permet de définir la taille d'un côté de l'écran</param>
        public Screen(int size) {
            this.height = size;
            this.width = size;
        }

        /// <summary>
        /// Constructeur de la classe, crée une fenêtre rectangulaire
        /// </summary>
        /// <param name="width">Permet de définir la longueur de l'écran</param>
        /// <param name="height">Permet de définir la largeur de l'écran</param>
        public Screen(int width, int height) {
            this.width = width;
            this.height = height;
        }


        /* OLD WAY
        /// <summary>
        /// Permet d'afficher l'écran dans la console
        /// </summary>
        public void Display() {
            List<string> lines = BuildBorder(); // on crée les bord de l'écran
            var layers = this.layers.OrderBy(x => x.Key).Select(x => x.Value).ToList(); // on trie les éléments à afficher en fonct
            foreach (var elements in layers)
            {
                foreach (KeyValuePair<Coordinates, string[]> element in elements) 
                {
                    int index = element.Key.x + 1;
                    for (int i = 0; i < element.Value.Count(); i++)
                    {
                        if (index + element.Value[i].Length > this.width)
                        {
                            string celuiQuiDepassePas = element.Value[i].Substring(0, (this.width - index) -1);
                            lines[element.Key.y + 1 + i] = lines[element.Key.y + 1].Remove(index, celuiQuiDepassePas.Length)
                                                                                .Insert(index, celuiQuiDepassePas);

                            Queue<string> file = new();
                            file.Enqueue(element.Value[i].Substring(celuiQuiDepassePas.Length, element.Value[i].Length - celuiQuiDepassePas.Length));
                            int lineIndex = 1;
                            while(file.Count() != 0) {
                                string elmt = file.Dequeue();
                                if (elmt.Length > this.width)
                                {
                                    Console.WriteLine(element.Value[i]);
                                    celuiQuiDepassePas = elmt.Substring(0, this.width - 2);
                                    lines[element.Key.y + 1 + lineIndex] = lines[element.Key.y + 1 + lineIndex]
                                                                                .Remove(1, celuiQuiDepassePas.Length)
                                                                                .Insert(1, celuiQuiDepassePas);

                                    file.Enqueue(elmt.Substring(celuiQuiDepassePas.Length, elmt.Length - celuiQuiDepassePas.Length));
                                } else
                                    lines[element.Key.y + 1 + lineIndex] = lines[element.Key.y + 1 + lineIndex].Remove(1, elmt.Length)
                                                                                                                .Insert(1, elmt);
                                lineIndex++;
                            }

                        } else{
                            lines[element.Key.y + 1 + i] = lines[element.Key.y + 1].Remove(index, element.Value[i].Length)
                                                                                .Insert(index, element.Value[i]);
                        }
                    }
                }
            }
            Console.Write(string.Join("", lines));
            Console.SetCursorPosition(0, 0);
        }
        */

        /// <summary>
        /// Permet d'afficher l'écran dans la console
        /// </summary>
        public void Display()
        {
            List<Element> elements = new List<Element>();
            if (elementsClone.Count <= 0)
            {
                string background = string.Join("", BuildBorder()); // on crée les bord de l'écran
                Console.Write(background); //On affiche le bords
            }

            bool recall = false; //Permet de savoir si l'éran doit être actualiser
            
            foreach (int layer in layers.Keys.OrderBy(x => x))
            {
                for (int n = 0; n < layers[layer].Count(); n++)
                {
                    bool update = true;
                    foreach (Element element in elementsClone)
                    {
                        if (element.CompareTo(layers[layer][n])) update = false;
                    }

                    if (!update) continue;
                        
                    for (int i = 0; i < layers[layer][n].text.Count(); i++)
                    {
                        if (layers[layer][n].animation != Animation.None && layers[layer][n].animationIndex[i] > 0) //Si un element doit être actualisé
                        {
                            layers[layer][n].animationIndex[i]--;
                            recall = true;
                        }

                        SetCursorElement(layers[layer][n], layers[layer][n].text[i], i);
                        
                        Console.ForegroundColor = layers[layer][n].foreground;
                        Console.BackgroundColor = layers[layer][n].background;

                        if (layers[layer][n].animationIndex[i] >= layers[layer][n].text[i].Length) layers[layer][n].animationIndex[i] = -1;

                        if (layers[layer][n].animationIndex[i] != -1)
                            Console.Write(string.Join("", layers[layer][n].text[i].Take(layers[layer][n].text[i].Length - layers[layer][n].animationIndex[i])));
                        else Console.Write(layers[layer][n].text[i]);

                        elements.Add(layers[layer][n]);
                        //Reset des couleurs
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.BackgroundColor = ConsoleColor.Black;
                    }

                    if (layers[layer][n].temp)
                    {
                        this.layers[layer].RemoveAt(n);
                    }
                }

            }
            Console.SetCursorPosition(0, 0);

            if (recall) 
            {
                Thread.Sleep(100);
                Display();
            }
            elementsClone = elements;
        }

        /// <summary>
        /// Permet d'ajouet un élément sans spécifier de layer
        /// </summary>
        /// <param name="element">L'élément à ajouter</param>
        public void Add(Element element) {
            this.Add(element, 0);
        }

        /// <summary>
        /// Permet d'ajouter un élément
        /// </summary>
        /// <param name="element">L'élément à ajouter</param>
        /// <param name="layer">Le layer sur lequel ajouter l'élément</param>
        public void Add(Element element, int layer) {
        if (!layers.ContainsKey(layer))
            layers.Add(layer, new List<Element>());
        layers[layer].Add(element);
        }

        /// <summary>
        /// Permet de supprimer les élément situer à une coordonnée
        /// </summary>
        /// <param name="coordinates">La coordonnée où supprimer les éléments</param>
        public void Delete(Coordinates coordinates) {
            foreach (KeyValuePair<int, List<Element>> layer in this.layers)
            {
                DeleteElement(layer.Value.FirstOrDefault(x => x.coordinates == coordinates));
                layer.Value.Remove(layer.Value.FirstOrDefault(x => x.coordinates == coordinates));
            }
        }

        /// <summary>
        /// Permet de supprimer les élément situer à une coordonnée sur un layer
        /// </summary>
        /// <param name="coordinates">La coordonnée où supprimer les éléments</param>
        /// <param name="layer">Le layer sur lequel supprimer l'élément</param>
        public void Delete(Coordinates coordinates, int layer) {
            DeleteElement(layers[layer].FirstOrDefault(x => x.coordinates == coordinates));
            this.layers[layer].Remove(layers[layer].FirstOrDefault(x => x.coordinates == coordinates));
        }

        /// <summary>
        /// Permet de supprimer un layer entier
        /// </summary>
        /// <param name="layer">Le layer à supprimer</param>
        public void DeleteLayer(int layer) {
            if (this.layers.ContainsKey(layer)) 
            {
                foreach (Element element in this.layers[layer])
                {
                    DeleteElement(element);
                }
                this.layers.Remove(layer);
            }
        }

        public void DeleteElement(Element element)
        {
            this.Add(element.GetEmptyClone(), 0);
        }

        private void SetCursorElement(Element element, string text, int index)
        {
            if (element.placement == Placement.topLeft)
                Console.SetCursorPosition(element.coordinates.x, element.coordinates.y + index);

            else if (element.placement == Placement.mid)
                Console.SetCursorPosition(element.coordinates.x - (text.Length/2), (element.coordinates.y - element.text.Length/2) + index);

            else if (element.placement == Placement.topRight)
                Console.SetCursorPosition(element.coordinates.x - text.Length, element.coordinates.y + index);
                        
            else if (element.placement == Placement.botLeft)
                Console.SetCursorPosition(element.coordinates.x, (element.coordinates.y+index) - text.Length);

            else if (element.placement == Placement.botLeft)
                Console.SetCursorPosition(element.coordinates.x - text.Length, (element.coordinates.y+index) - text.Length);

        }
        
        /// <summary>
        /// Permet de clear l'écran
        /// </summary>
        public void Clear() {
            this.layers.Clear();
            Console.Clear();
            string background = string.Join("", BuildBorder()); // on crée les bord de l'écran
            Console.Write(background); //On affiche le bords
        }

        /// <summary>
        /// Permet de creer les bords de l'écran
        /// </summary>
        private List<string> BuildBorder() {
            string top = "+" + new String('-', this.width-2) + "+\n";
            string mid = "|" + new String(' ', this.width-2) + "|\n";
            List<string> lines = Enumerable.Repeat(mid, this.height-2).ToList();
            lines.Insert(0, top);
            lines.Add(top);
            return lines;
        }

        public int Choice(string[] choixArray, int height)
        {
            List<Element> choixElements = new List<Element>();
            int space = this.width / (choixArray.Length+1); 
            for(int i = 0; i < choixArray.Length; i++)
            {
                Element choixElement = new Element(new string[1]{choixArray[i]}, 
                                                    new Coordinates(space * (i+1), height),
                                                    Animation.None,
                                                    Placement.mid, ConsoleColor.White, ConsoleColor.Black);
                choixElements.Add(choixElement);
                this.Add(choixElement, 2);
            }
            this.Display();
            return Select(choixElements.ToArray(), 2);
        }

        public int Select(Element[] elementArray, int layer)
        {
            int choix = 0;
            while(true)
            {
                ConsoleKey key = Console.ReadKey().Key;
                if (key == ConsoleKey.RightArrow || key == ConsoleKey.UpArrow) {
                    choix++;
                } else if (key == ConsoleKey.LeftArrow || key == ConsoleKey.DownArrow) {
                    choix--;
                } else if (key == ConsoleKey.Enter) {
                    break;
                }

                if (choix < 0) choix = elementArray.Length-1;
                if (choix >= elementArray.Length) choix = 0;
                
                
                for (int i = 0; i < elementArray.Length; i++)
                {
                    if (i == choix)
                    {
                        elementArray[i].foreground = ConsoleColor.Black;
                        elementArray[i].background = ConsoleColor.White;
                    }
                    else
                    {
                        elementArray[i].foreground = ConsoleColor.White;
                        elementArray[i].background = ConsoleColor.Black;
                    }
                }
                Console.Write(choix);
                this.Display();
            }
            this.DeleteLayer(layer);
            return choix;
        }
    }
}