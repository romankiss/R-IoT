# Návod na inštaláciu SW na programovanie ESP32 čipov s C#
## MS Visual Studio 2022 

Čipy sa budú programovať v jazyku C#. Ako editor nám poslúži MS Visual Studio 2022. Inštalačku si stiahnite tu: [MSVS22](https://visualstudio.microsoft.com/cs/thank-you-downloading-visual-studio/?sku=Community&channel=Release&version=VS2022&source=VSLandingPage&cid=2030&passive=false)     
OSko by malo byť ideálne Windows10/11, inštalácia cez emulátor na Linux neni vylúčená no spojená pravdepodobne s mnohými problémami.     
Nebudem dopodrobna opysovať postup lebo verím, že v tomto kroku by sa nemalo nič pokaziť a po chvíli by sa mohlo ukázať dačo takéto:    

![pohľad na VS Installer](https://github.com/romankiss/R-IoT/assets/59760649/f5654bcc-73ce-482c-a952-e50f288cfcc8)
![pohľad na ponuku pri otvorení MSVS22](https://github.com/romankiss/R-IoT/assets/59760649/a125473a-1d60-401b-9c4c-96a69497bf43)

### Abdejt

**POZOR!!!   - ak už máš inštalované štúdio kukni verziu - 17,7,4 je najnovšia v dobe písania tohoto návodu.   
Pozri dostupnosť apdejtu!!!  
Apdejty bude písať dakte v installery, alebo bež do štúdia>karta hore Help>Check for updates>     
...(na urobenie tohoto kroku trba otvoriť "editačný mód", čiže vytvoriť si nový projekt. Ponuka nám bude dávať rôzne konfigi... v podstate je jedno ktorý konfig vyberieme pre tento skúšobný projekt - ide o to dostať sa do editore a urobiť apdejt.))**

![vyberanie z ponuky templejtov: vybarme console app](https://github.com/romankiss/R-IoT/assets/59760649/7361d1a2-f2f3-4d8d-8998-3d8b545810b2)

Vybranie templejtu... napr. console app > Next.

![vyberanie kam chceme náš projekt uložiť](https://github.com/romankiss/R-IoT/assets/59760649/9dd06c7f-d293-495c-8870-d6b5591b0131)

Vybranie ukladania projektu. Ideále sem napíš: "C:\Projects\nanoFramework":

![vyberieme / vytvoríme zložku C:\\Projects\nanoFramework](https://github.com/romankiss/R-IoT/assets/59760649/9765d1c8-3899-44d3-a869-d0907269e93d)

**Teraz sme v "editačnom móde".**
![editačný mód](https://github.com/romankiss/R-IoT/assets/59760649/e869a40b-06e5-41a7-83e2-75396ea2e196)

**Karta Help > Check for updates > atd´... urob proste apdejt!**      
![karta help](https://github.com/romankiss/R-IoT/assets/59760649/c1e38537-a17c-42e8-8331-b29de003d2d8)





<p color="red"><strong>Pokiaľ máš inštalačku 22 Community neznamená to že máš jej najnovšiu verziu.  <br>   
Ak nemáš najnovšiu verziu rozšírenie čo budeme inštalovať neskôr nepôjde ukázať v zozname rozšírení!!!</strong></p>



## nanoFramework

Oukej, čiže teraz ked´ už máme najnovšiu verziu štúdia stiahnutú ideme si stiahnuť framework .NET prispôeobený pre malé zariadenia s obmädzenou pamäťou a tak - nanoFramework.   
Prvým krokom bude mať pred sebou zapatý "editačný mód" MSVS22.   
![editačný mód](https://github.com/romankiss/R-IoT/assets/59760649/e869a40b-06e5-41a7-83e2-75396ea2e196)   
Rozkliknime kertu Extensions > Manage extensions   
![karta Extensions](https://github.com/romankiss/R-IoT/assets/59760649/98b6f4f8-5986-4e30-a320-6dfc25a97e25)   
V ľavo tohoto okna máme kategórie: Installed, Online, Updates...     
Klik na kategóriu Online > do vyhľadávanie v pravo hore napíš "nano"    
Vyhľadávanie by malo vyhodiť jediný výsledok ".NET nanoFramework Extension":
![Hl´adanie nano Frejmworku](https://github.com/romankiss/R-IoT/assets/59760649/88be4046-8dbf-4072-96f4-90ac6b08fcb9)    

<p color="red"> Pokiaľ ti vyhľadávanie nenašlo to čo malo určite sa uisti, že máš naj. verziu MSVS.  <br>
Príp. skús vypnúť a znova zapnúť celé MSVS. </p>

Urob všetko čo od teba budú chcieť, nainštaluj nanoFramework.
Pokiaľ sa všetko podarilo tak kategória Installed by mala vypadať cca dajako takto:  

![Kategória Installed ext.](https://github.com/romankiss/R-IoT/assets/59760649/b7c0fef6-ad78-444e-8c66-51cf1d292524)  

## Napaľovačka firmwéru

Posledný SW čo bude treba stiahnuť je nám na to, aby sme napálili firmwér do našich čipov.  
Budeme potrebovať WIN Power shell (Modré CMD).  

![PS s vlepeným komandom](https://github.com/romankiss/R-IoT/assets/59760649/f07e49ed-7203-40c6-b371-75cf9cec482a)   
Do riadku napíšeme toto:    dotnet tool install -g nanoff        
A potom toto:  nanoff --target ESP32_PICO --serialport COM5 --update   (port nahrd´)

