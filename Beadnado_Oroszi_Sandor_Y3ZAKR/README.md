# Mozgásfigyelő – .NET MAUI beadandó

**Készítette:** Oroszi Sándor (Y3ZAKR)
**Tárgy:** Mobilprogramozás
**Technológia:** .NET 10, .NET MAUI, C#, XAML
**Célplatform:** Android (emulátoron és valódi eszközön is futtatható)

## Az alkalmazás rövid leírása

A Mozgásfigyelő egy kétfüles mobilalkalmazás, amely a telefon szenzorait használja:

1. **Lépésszámláló** – a gyorsulásmérő (Accelerometer) adataiból számolja a megtett lépéseket.
   - Beállítható lépéscél (csúszkával vagy beírással) és lépéshossz.
   - Valós időben mutatja a gyorsulás X / Y / Z értékét és nagyságát.
   - Haladásjelző sáv, megtett táv kilométerben.
   - **A cél elérésekor a mérés automatikusan leáll**, a szám zöldre vált, és megjelenik egy gratuláció.
   - A befejezett mérések egy listába kerülnek (kezdés ideje, időtartam, lépésszám).
2. **Iránytű** – a Compass szenzor (magnetométer) alapján egy tű mindig észak felé mutat,
   és kiírja az irányt fokban és égtájként (É, ÉK, K, …). Ha a telefon észak felé néz, a felirat zöld lesz.

## Lefedett témakörök

| # | Témakör | Megvalósítás |
|---|---------|--------------|
| 2 | .NET MAUI architektúra, projekt felépítése | Rendezett mappastruktúra, Shell alapú alkalmazás |
| 3 | GUI-tervezés XAML nyelven | Két oldal: `Grid`, `Border`, `CollectionView`, `Slider`, `Entry`, `ProgressBar`, `Ellipse`, `BoxView` |
| 4 | Adatkapcsolás (DataBinding) | Egyirányú és kétirányú (`TwoWay`) kötés, `StringFormat`, saját értékkonverterek, `DataTrigger`, gyűjtemény kötése, `Rotation` kötése, fordítás idejű kötés (`x:DataType`) |
| 5 | MVVM tervezési minta | `ViewModelBase` (`INotifyPropertyChanged`), külön Model, ViewModel és View réteg |
| 11 | Szenzoradatok lekérdezése | `Accelerometer` (lépésszámlálás), `Compass` (iránytű) |

A **Commands (6.)** és a **Dependency Injection (7.)** a feladat szerint szándékosan nincs használva:
- a gombok `Clicked` eseménykezelői a code-behindban csak meghívják a ViewModel megfelelő metódusát, üzleti logika nincs bennük;
- a ViewModelek XAML-ből jönnek létre (`<ContentPage.BindingContext>`), nem DI konténerből.

## Projektstruktúra

```
Beadnado_Oroszi_Sandor_Y3ZAKR/
├── App.xaml(.cs)              – alkalmazás belépési pont
├── AppShell.xaml(.cs)         – navigáció: TabBar két füllel
├── Models/
│   └── StepSession.cs         – egy befejezett mérés adatai
├── ViewModels/
│   ├── ViewModelBase.cs       – INotifyPropertyChanged + SetProperty segédfüggvény
│   ├── StepCounterViewModel.cs – lépésszámlálás, cél, mérések listája
│   └── CompassViewModel.cs    – iránytű adatai
├── Views/
│   ├── StepCounterPage.xaml(.cs)
│   └── CompassPage.xaml(.cs)
├── Converters/
│   ├── BoolToColorConverter.cs  – bool → szín (pl. cél elérve = zöld)
│   └── InvertedBoolConverter.cs – bool megfordítása (pl. Start gomb tiltása mérés közben)
└── Resources/                 – stílusok, színek, ikonok, betűtípusok
```

## Működés röviden

### Adatkötés és MVVM
A ViewModel minden megjelenített tulajdonsága a `SetProperty` metóduson keresztül változik, amely
`PropertyChanged` eseményt küld. A XAML-ben lévő `{Binding ...}` kifejezések erre feliratkoznak, így a
felület automatikusan frissül. A számolt tulajdonságokhoz (például `Progress`, `IsGoalReached`, `DistanceKm`)
a ViewModel külön értesítést küld, amikor az alapértékük (`Steps`, `Goal`, `StepLengthCm`) változik.

### Lépésdetektálás
A gyorsulásvektor nagyságát (`√(x² + y² + z²)`, g egységben) egyszerű aluláteresztő szűrővel simítjuk.
Lépésnek számít, ha a simított érték 1,20 g fölé emelkedik, és előtte 1,05 g alá esett vissza.
Két lépés között legalább 300 ms-nak el kell telnie, így a zaj nem számít többszörös lépésnek.

### Automatikus leállás
Minden új lépés után a ViewModel ellenőrzi, hogy elérte-e a célt. Ha igen, meghívja a `Stop()` metódust:
leállítja a szenzort, elmenti a mérést a listába, és kiírja a gratulációt.

### Szálkezelés
A szenzor eseményei nem feltétlenül a UI szálon érkeznek, ezért a ViewModel a
`MainThread.BeginInvokeOnMainThread` segítségével frissíti a kötött tulajdonságokat.
Az iránytű csak akkor fut, amikor az oldala látszik (`OnAppearing` / `OnDisappearing`), így kíméli az akkumulátort.

## Futtatás

1. Nyisd meg a `Beadnado_Oroszi_Sandor_Y3ZAKR.slnx` fájlt Visual Studio 2026-ban
   (szükséges: .NET MAUI és Android workload).
2. Válassz egy Android emulátort vagy USB-n csatlakoztatott telefont.
3. Indítsd el (F5).

Parancssorból:

```bash
dotnet build -t:Run -f net10.0-android
```

## Tesztelés emulátoron

Az emulátor oldalsávjában: **⋯ (Extended controls) → Virtual sensors**.

| Funkció | Lépések | Elvárt eredmény |
|---------|---------|-----------------|
| Szenzor él | Start, majd a Device Pose fülön forgasd a telefont | Az X / Y / Z értékek változnak |
| Lépésszámlálás | Mérés közben nyomd meg a „+1 lépés (emulátor teszt)” gombot | Nő a lépésszám, a haladásjelző és a táv |
| TwoWay kötés | Húzd a cél csúszkáját, vagy írj számot a mezőbe | A kettő együtt változik |
| Automatikus leállás | Állítsd a célt 5-re, Start, majd 5× „+1 lépés” | A mérés leáll, a szám zöld, új sor jelenik meg a listában |
| Iránytű | Iránytű fül, Device Pose → Yaw állítása | A tű észak felé fordul, 0° körül zöld az „É” |

Emulátoron a forgatás nem ad lépést, mert a gyorsulás nagysága közben végig kb. 1 g marad.
Valódi lépésszámláláshoz valódi telefon kell.
