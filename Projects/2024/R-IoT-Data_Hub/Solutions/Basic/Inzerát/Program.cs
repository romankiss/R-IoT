
// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");
if (File.Exists("C://Users//ratze//Documents//Inzerát.txt")) {
    string inzerContent = File.ReadAllText("C://Users//ratze//Documents//Inzerát.txt");
    string[] inzerDeleny = inzerContent.Split(new char[] { '#' });
    Console.WriteLine("Názov: " + inzerDeleny[1] + " Cena: " + inzerDeleny[2]);
};
WindowsInput.MouseButton.LeftButton.