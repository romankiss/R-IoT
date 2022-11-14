Základné pojmy

Riadenie prístupu: Systém, ktorý určuje, kto, kedy a kde môžu ľudia vstúpiť alebo opustiť zariadenie alebo oblasť. Tradičnou formou kontroly prístupu je používanie zámkov dverí, ale moderná kontrola prístupu môže zahŕňať elektronické systémy a bezdrôtové zámky. Kontrola prístupu sa môže vzťahovať aj na kybernetickú bezpečnosť.
Access Point: Uzol Wi-Fi, ktorý umožňuje používateľom vstup do siete, zvyčajne LAN.
API: Programovacie prostredie aplikácií.
Arduino: Jednodoskový mikrokontrolér používaný na prototypovanie bez toho, aby ste sa museli zaoberať doskami na pečenie alebo spájkovaním.
Broker: Sprostredkovateľ správ IoT je procesor na spracovanie udalostí a príkazov naprieč zariadeniami, systémami a procesmi pomocou pracovného toku a analýzy.
MQTT: MQTT je štandardný protokol na odosielanie správ pre internet vecí (IoT). Je navrhnutý ako extrémne ľahký prenos správ na publikovanie/predplatenie, ktorý je ideálny na pripojenie vzdialených zariadení s malou kódovou stopou a minimálnou šírkou pásma siete.

Technológia

IoT je počítačový periférny systém, ktorý interaguje s mechanickými alebo digitálnymi strojmi, predmetmi. Pre spotrebiteľov je internet vecí známejší ako koncept smart-home (osvetlenie, automatizácia, kúrenie / klimatizácia, informačné médiá bezpečnostného systému atď.).
Bezdrôtový prenos na krátke vzdialenosti, napríklad:
Wi-Fi je technológia pre lokálne siete (LAN) založená na štandarde IEEE 802.15.4, ktorá to umožňuje gadget môžu komunikovať prostredníctvom centralizovaného prístupového bodu alebo medzi nimi gadget rozdielny bezdrôtový/ bezdrôtové.
Li-Fi (Light-Fidelity), technológia dátovej komunikácie bezdrôtový ktorá je podobná sieti Wi-Fi, ale na zvýšenie rýchlosti používa ľahké médiá šírky pásma.
RFID (Rádiofrekvenčná identifikácia), táto technológia sa využíva na čítanie údajov elektromagnetické polia.
NFC (Komunikácia v blízkom okolí), protokol, ktorý umožňuje vzájomnú komunikáciu dvoch elektronických zariadení v rozsahu 4 cm.
Bezdrôtové pripojenie stredného dosahu, napríklad:
LTE Advance, vysokorýchlostná dátová komunikácia (vysoká rýchlosť) pre zariadenia mobilnej siete.
Bezdrôtové pripojenie na veľké vzdialenosti, napríklad:
Nízkoenergetické širokospektrálne siete (LPWAN), navrhnutá pre dátovú komunikáciu na veľké vzdialenosti, šetrí viac energie a náklady v procese prenosu.
Terminál s veľmi malou apertúrou je obojsmerná satelitná pozemná stanica s anténou, ktorá je menšia ako 3,8 metra. Väčšina antén VSAT má dosah od 75 cm do 1,2 m 
Wired, napríklad:
Ethernet, je sieťový štandard, ktorý používa skrútený pár a optická vlákna ktoré je spojené pomocou čísla húb a spínač.
Komunikácia po elektrickej sieti (PLC), komunikačná technológia využívajúca elektrické káble na prenos energie a údajov.

Proces telemetria

To je známe ako telemetria do systému, ktorý umožňuje sledovať, sprostredkovanie fyzikálnych veličín alebo chemických prostredníctvom dát sa prenáša do centrálneho riadenia. Telemetrický systém sa zvyčajne vykonáva pomocou bezdrôtovej komunikácie.

Naše riešenie:

V smetných nádobách budú umiestnené senzory, ktoré budú zbierať informácie o zaplnení týchto nádob. Senzory sú naplánované aby boli napojené na arduina. Tieto arduina budú odosielať info do Raspberry mikrokontrolerov v ktorých bude toto info spracované a odoslané pre konečného užívateľa. Tento užívateľ bude vedieť dať pokyn na základe info zo smetných nádob, či je za potreby ísť smetnú nádobu vyniesť alebo nie.

Edge

Edge internetu vecí (IoT) je miesto, kde senzory a zariadenia prenášajú údaje do siete v reálnom čase. IoT edge computing rieši problémy s latenciou spojené s cloudom, keďže dáta sú spracovávané bližšie k ich východiskovému bodu. Spolu so zníženou latenciou prináša IoT okrajová architektúra zvýšenú bezpečnosť a hladší zážitok pre koncového používateľa.
 
V sieti s vysokou priepustnosťou, ako je napríklad 5G, možno IoT edge použiť na takmer okamžité spracovanie veľkého množstva údajov, čím sa pre používateľa vytvorí pohlcujúcejší a komplexnejší zážitok. Zároveň, aj keď sa prenášajú relatívne malé množstvá dát, IoT edge dokáže urýchliť prácu strojov a iných zariadení, ktoré majú vplyv na bezpečnosť ľudí, čím zaistí bezpečnosť operátorov a ostatných.

Komponenty v našej architektúre:

Senzor
Edge Node – Raspberry Pi
Operačný systém - Raspbian 
Raspbian je bezplatný operačný systém založený na Debiane optimalizovaný pre hardvér Raspberry Pi. Operačný systém je súbor základných programov a pomôcok, vďaka ktorým funguje vaše Raspberry Pi. Raspbian však poskytuje viac než len čistý operačný systém: prichádza s viac ako 35 000 balíkmi, predkompilovaným softvérom zabaleným v peknom formáte pre jednoduchú inštaláciu na vaše Raspberry Pi. 
Počiatočná zostava viac ako 35 000 balíčkov Raspbian, optimalizovaných pre najlepší výkon na Raspberry Pi, bola dokončená v júni 2012. Raspbian je však stále v aktívnom vývoji s dôrazom na zlepšenie stability a výkonu čo najväčšieho počtu balíčkov Debianu.
Na prístup k Raspberry budeme používať funkciu VNC (Virtual Network Computing).
Virtual Network Computing je samozrejmou možnosťou pri práci s platformami Linux. Ide o multiplatformovú technológiu vzdialenej pracovnej plochy, ktorú možno použiť na väčšine operačných systémov. Na trhu je naozaj veľké množstvo produktov. Len málo produktov je zadarmo a len málo komerčných. Väčšina nových distribúcií Raspberry Pi však prichádza s ReavVNC, ktoré podporuje cloudové pripojenia priamo dovnútra. To vám umožňuje pripojiť Pi nielen v rámci siete, ale aj cez internet. RealVNC dáva používateľom slobodu pripojiť sa odkiaľkoľvek. Vďaka tejto flexibilite je táto možnosť výraznejšia ako ostatné. 
Node-Red 
Pre vývoj komplexných IoT riešení je na trhu k dispozícií aj open-source vizuálno-programovací nástroj Node-RED (https://nodered.org/). Tento nástroj pochádza z dielne spoločnosti IBM, ktorá sa aktívne podieľa na vývoji IoT infraštruktúry a IoT štandardov. Jeho veľkou výhodou je,  že funguje vo webovom prehliadači.

Node-RED je možné nainštalovať: Lokálny počítač, Na doskový počítač typu Raspberry Pi, Android zariadenie, Do cloudu (napríklad Amazon, Google)

Komunikácia

MQTT 
MQTT je štandardný protokol na odosielanie správ OASIS pre internet vecí (IoT). Je navrhnutý ako extrémne ľahký prenos správ na publikovanie/predplatenie, ktorý je ideálny na pripojenie vzdialených zariadení s malou kódovou stopou a minimálnou šírkou pásma siete. MQTT sa dnes používa v širokej škále priemyselných odvetví, ako je automobilový priemysel, výroba, telekomunikácie, ropa a plyn atď.
JSON
JSON (JavaScript Object Notation) je jednoduchý formát na výmenu údajov. Pre ľudí je ľahké čítať a písať. Pre stroje je ľahké analyzovať a generovať. Je založený na podmnožine štandardu JavaScript Programming Language Standard ECMA-262 3. vydanie – december 1999. JSON je textový formát, ktorý je úplne nezávislý na jazyku, ale používa konvencie, ktoré poznajú programátori z rodiny jazykov C, vrátane C, C++, C#, Java, JavaScript, Perl, Python a mnoho ďalších. Vďaka týmto vlastnostiam je JSON ideálnym jazykom na výmenu údajov.
