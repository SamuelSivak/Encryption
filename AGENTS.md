# Encryption Project - Agent Guidelines

## Hlavný režim práce
- Kód len navrhuj a ukáž ho v odpovedi. Súbory nemeň a príkazy nespúšťaj, kým o to výslovne nepožiadam.
- Pred návrhom si vždy prečítaj súvisiace súbory (controller, služby, DTO, modely), aby si zachoval existujúci štýl a architektúru.
- Ak niečo nevieš alebo si nie si istý, povedz to. Nevymýšľaj si triedy, metódy ani konfiguráciu, ktorú si v projekte nevidel.
- Ak zmena ovplyvňuje viac súborov, najprv stručne vypíš, ktoré súbory sa zmenia a prečo.

## Formát odpovede
- Odpovedaj po slovensky, komentáre v kóde píš po anglicky.
- Najprv stručne popíš plán, potom ukáž kód.
- Pri úprave existujúceho súboru uveď názov súboru a ukáž len zmenenú časť, nie celý súbor (okrem malých súborov).

## Konvencie kódu
- C# / .NET, ASP.NET Core, async/await všade, kde sa dá.
- Pomenovanie: PascalCase pre triedy a metódy, _camelCase pre privátne polia.
- Závislosti vstrekuj cez konštruktor, nepoužívaj statické triedy so stavom.
- Dodržuj existujúce rozdelenie: Controllers, Services, DTO, Models (SQL a MongoDB).
- Nové knižnice (NuGet balíčky) nepridávaj bez opýtania.
- Tajomstvá (kľúče, heslá, connection stringy) nikdy nevpisuj do kódu, patria do konfigurácie.

## Overovanie (len keď o zmenu požiadam)
- Po schválenej úprave spusti `dotnet build` a ak build zlyhá, ukáž chybu a navrhni opravu.
- Ak existujú testy, spusti `dotnet test`.
- Príkazy mimo `dotnet build`, `dotnet test` a čítania Git stavu vždy najprv konzultuj.

## Prehľad projektu
ASP.NET Core Web API na vytváranie a dešifrovanie šifrovaných odkazov (URL) pomocou passphrase.

## Technológie
- .NET 10.0
- SQL Server (metadáta)
- MongoDB (zašifrovaný obsah)
- Redis (cache)
- JWT autentifikácia
- AES-GCM šifrovanie s odvodením kľúča cez PBKDF2

## Požiadavky na spustenie
1. SQL Server beží lokálne.
2. MongoDB beží lokálne alebo je connection string v `appsettings.json`.
3. Redis beží lokálne alebo je connection string v `appsettings.json`.

## Spustenie
```bash
# Build a spustenie
dotnet run --project Encryption\Encryption.csproj

# Obnovenie balíčkov
dotnet restore Encryption\Encryption.csproj
```

## Konfigurácia databáz
Connection stringy sú v `appsettings.json`:
- `SqlMaster`: metadáta (SQL Server)
- `MongoDb`: zašifrovaný obsah (MongoDB)
- `Redis`: cache

## API endpointy
- `POST /api/links/create` - vytvorenie šifrovaného odkazu
- `POST /api/links/{shortCode}/decrypt` - dešifrovanie odkazu
- `GET /api/links/my-links` - odkazy prihláseného používateľa

## Bezpečnostné vlastnosti
- AES-GCM šifrovanie obsahu URL
- PBKDF2 na odvodenie kľúča z passphrase
- Vytváranie odkazov vyžaduje JWT
- Sledovanie IP adries a logovanie prístupov
- Expirácia odkazov a limity počtu prístupov
- Burn-on-read (zničenie po prvom použití)

## Dôležité poznámky
- Služba vyžaduje dostupný SQL Server aj MongoDB.
- Redis slúži len na cache a nie je kritický pre základnú funkčnosť.
- Passphrase sa nikdy neukladá, používa sa len na dešifrovanie.