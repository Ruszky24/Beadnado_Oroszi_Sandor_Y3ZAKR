# Mobile Motion Tracker – .NET MAUI labor beadandó

**Készítette:** Oroszi Sándor (Y3ZAKR)
**Technológia:** C#, .NET 10, .NET MAUI, XAML
**Célplatform:** Android

## Az alkalmazás

Egyoldalas, görgethető (`ScrollView`) alkalmazás három szekcióval:

1. **Helymeghatározás** – a *Helyzet lekérdezése* gomb a `Geolocation` API-val lekéri a pozíciót
   (szélesség/hosszúság 4 tizedesjeggyel). Kéri és kezeli a `LocationWhenInUse` engedélyt;
   hiba, engedélyhiány, kikapcsolt vagy nem elérhető helyadat esetén érthető hibaüzenetet ír ki.
2. **Gyorsulásmérő** – `Switch`-csel indítható/leállítható. X, Y, Z értékek g-ben, 2 tizedesjeggyel,
   a = √(x² + y² + z²), d = |a − 1|, mozgásbesorolás:
   d < 0,15 *Nyugalomban*, 0,15 ≤ d < 0,50 *Enyhe mozgás*, d ≥ 0,50 *Erős mozgás*.
   Leállításkor az utolsó értékek láthatók maradnak.
3. **Iránytű** – külön `Switch`. Mágneses irány egész fokra kerekítve, 8 égtájra besorolva
   (alsó határ a tartományhoz tartozik, 360° = 0°). Nem támogatott szenzor esetén:
   „Az iránytű nem támogatott ezen az eszközön.” Extra: forgó grafikus iránytű-lap.

## Szerkezet (MVVM)

| Mappa | Állomány | Feladat |
|-------|----------|---------|
| Models | `MotionData.cs` | A gyorsulás X, Y, Z értékei (+ nagyság, eltérés) |
| Views | `MainPage.xaml`, `MainPage.xaml.cs` | Felület, összekötés a ViewModellel |
| ViewModels | `MainViewModel.cs`, `ViewModelBase.cs` | Adatok, állapotok, számítások, műveletek; `INotifyPropertyChanged` |
| Converters | `MotionStateColorConverter.cs` | Mozgásállapot → szín (zöld / narancssárga / piros) |

- **Data Binding:** a mért értékek `Mode=OneWay`, a két `Switch.IsToggled` `Mode=TwoWay` kötéssel
  kapcsolódik az `IsAccelerometerOn` / `IsCompassOn` propertykhez.
- **IValueConverter:** XAML-erőforrásként regisztrálva, a mozgásállapot `Label` `TextColor`-jához kötve.

## Megjegyzés

A szenzorok nem minden emulátorban érhetők el; az Android emulátor *Extended controls → Virtual sensors*
és *Location* paneljén lehet értékeket szimulálni.
