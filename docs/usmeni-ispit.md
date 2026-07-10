# Priprema za usmeni ispit — WebIde

Ovaj dokument objašnjava **ključne ideje i komade koda** koje moraš znati usmeno
objasniti. Naglasak je na **kontrolerima** i na **cijelom putu koda od klika
"Submit" do rezultata na ekranu**. Uz svaki koncept stoji i datoteka gdje se
nalazi u projektu, da možeš pokazati kod.

---

## 1. Velika slika (30-sekundno objašnjenje)

> "WebIde je platforma za natjecateljsko programiranje. Korisnik piše kod u
> pregledniku, pošalje ga, kod se izvršava izolirano u Docker sandboxu, a rezultat
> se **uživo** vraća natrag u preglednik. Sustav je podijeljen na dva procesa: **web
> aplikaciju** (ASP.NET MVC) koja prima zahtjeve, i **worker** (pozadinski servis)
> koji izvršava kod. Povezuje ih **Redis** — jednom kao red poslova, drugi put kao
> kanal za rezultate."

Zašto **dva procesa**, a ne jedan? Zato da izvršavanje nepouzdanog (korisnikovog)
koda ne blokira i ne ruši web aplikaciju. Web app ostaje brz i odgovara na
zahtjeve; teški, opasni posao (kompajliranje, pokretanje) radi izolirani worker.

### Projekti (slojevi) u rješenju

| Projekt | Uloga |
|---|---|
| `WebIde.Model` | Čiste domenske klase (Problem, Submission, User…) + enumi. Bez frameworka. |
| `WebIde.DAL` | `WebIdeDbContext` (EF Core), migracije, mapiranje na PostgreSQL. |
| `WebIde.Frontend` | ASP.NET MVC — **kontroleri**, Razor viewovi, repozitoriji, SignalR hub. |
| `WebIde.Worker` | Pozadinski servis koji vadi poslove iz Redisa i izvršava kod u sandboxu. |

Infrastruktura: **PostgreSQL** (trajni podaci), **Redis** (red poslova + pub/sub +
SignalR backplane), **Docker** (sandbox kontejneri), **nginx** (reverse proxy, TLS).

---

## 2. Kontroleri — što su i kako rade

**Kontroler** je klasa koja prima HTTP zahtjev, odradi logiku (obično preko
repozitorija/EF-a) i vrati odgovor — ili **View** (HTML) ili **JSON** ili
preusmjeravanje. Sve su izvedene iz `Controller`.

### 2.1 Rutiranje (routing)

Projekt koristi **atributno rutiranje** — ruta se piše iznad kontrolera/akcije:

```csharp
[Route("problems")]                       // prefiks za cijeli kontroler
public class ProblemController : Controller
{
    [Route("")]                           // GET /problems
    public IActionResult Index(string? sort) { ... }

    [Route("{id:int}")]                   // GET /problems/5  (id mora biti int)
    public IActionResult Details(int id) { ... }

    [HttpGet("create")]                   // GET /problems/create
    [HttpPost("create")]                  // POST /problems/create
    public IActionResult Create(...) { ... }
}
```

- `{id:int}` je **route constraint** — ruta se poklopi samo ako je `id` cijeli broj.
- `[HttpGet]` / `[HttpPost]` određuju **HTTP metodu** (dohvat vs. slanje podataka).

### 2.2 Dependency Injection (DI)

Kontroler u konstruktoru traži ovisnosti; ASP.NET ih **sam ubaci** (registrirane su
u `Program.cs`, npr. `builder.Services.AddScoped<ProblemRepository>()`).

```csharp
public ProblemController(ProblemRepository repo, SubmissionRepository submissions, WebIdeDbContext db)
{
    _repo = repo; _submissions = submissions; _db = db;
}
```

**Poanta za usmeni:** kontroler ne stvara sam bazu ili repozitorij (`new ...`) —
dobije ih izvana. To olakšava testiranje i mijenjanje implementacije.

### 2.3 Autorizacija

```csharp
[Authorize(AuthenticationSchemes = WebAuthSchemes.Cookies, Roles = "Admin,Manager")]
public IActionResult Create() { ... }
```

- `[Authorize]` = mora biti prijavljen.
- `Roles = "Admin,Manager"` = mora imati jednu od tih uloga.
- `AuthenticationSchemes = "Cookies,Identity.Application"` — bitan detalj ovog
  projekta: postoje **dvije prijave** (GitHub OAuth *i* korisničko ime/lozinka
  preko ASP.NET Identity), pa akcija prihvaća **obje sheme**.

### 2.4 Tipičan tok akcije (na primjeru `ProblemController.Create` POST)

```csharp
[HttpPost("create")]
[ValidateAntiForgeryToken]                       // 1. zaštita od CSRF-a
[Authorize(..., Roles = "Admin,Manager")]        // 2. autorizacija
public IActionResult Create(CreateProblemViewModel model)
{
    if (!ModelState.IsValid)                      // 3. validacija modela
        return View(model);                       //    (vrati formu s greškama)

    var problem = new Problem { Title = model.Title, ... };
    _db.Problems.Add(problem);                    // 4. spremi u bazu (EF Core)
    _db.SaveChanges();

    TempData["Flash"] = $"Problem \"{model.Title}\" created.";
    return RedirectToAction(nameof(Details), new { id = problem.Id }); // 5. PRG obrazac
}
```

Redoslijed za pamtiti: **antiforgery → autorizacija → validacija → rad s bazom →
Redirect (Post-Redirect-Get)**. PRG sprječava dvostruko slanje forme na refresh.

---

## 3. GLAVNO: cijeli put koda kroz evaluaciju (korak po korak)

Ovo je najvažniji dio za usmeni. Ispričaj ga kao priču u koracima. Sudjeluju:
`Problem/Details.cshtml` (preglednik) → `SubmissionController` → **Redis red** →
`SubmissionWorker` → `SandboxOrchestrator` (Docker) → `SubmissionEvaluator` →
baza + **Redis kanal** → `RedisSubscriptionService` → `ExecutionHub` (SignalR) →
preglednik.

```
 PREGLEDNIK                WEB APP (WebIde.Frontend)          WORKER (WebIde.Worker)
 ─────────                 ─────────────────────────          ──────────────────────
 Monaco editor
   │ klik Submit
   │ fetch POST /submissions ─►  SubmissionController.Submit
   │                              • provjere + kreira Submission (Pending)
   │                              • RPUSH na Redis red "submissions:queue"
   │  ◄─ { submissionId }         • vrati id
   │
   │ SignalR connect /hubs/execution
   │ invoke SubscribeToSubmission(id) ─► ExecutionHub  (join grupa "submission:{id}")
   │                                                          │ LPOP "submissions:queue"
   │                                                          │ status=Running → PUBLISH
   │                                                          │ RunAsync → Docker sandbox
   │                                                          │ Evaluate (verdict, score)
   │                                                          │ spremi ExecutionResult u bazu
   │                                                          │ PUBLISH "execution:{id}"
   │            RedisSubscriptionService  ◄── (Redis pub/sub) ┘
   │              │ hub.Clients.Group("submission:{id}").SendAsync("submissionUpdate", …)
   │  ◄───────────┘ (SignalR poruka)
   │ prikaži ACCEPTED / WRONG_ANSWER …
```

### Korak 1 — Preglednik šalje kod (`Views/Problem/Details.cshtml`)

Editor je **Monaco**. Na klik gumba skripta pošalje `fetch` POST na `/submissions`
s JSON tijelom i **antiforgery tokenom** u zaglavlju:

```js
const res = await fetch('/submissions', {
    method:  'POST',
    headers: { 'Content-Type': 'application/json',
               'RequestVerificationToken': token },      // CSRF token
    body: JSON.stringify({ problemId: PROBLEM_ID, language: currentLang, sourceCode })
});
const { submissionId } = await res.json();
trackSubmission(submissionId, consoleEl);                // otvori SignalR (Korak 3)
```

### Korak 2 — `SubmissionController.Submit` prima i stavlja u red

Datoteka: `WebIde.Frontend/Controllers/SubmissionController.cs`

```csharp
[HttpPost("")]
[Authorize]
[EnableRateLimiting("submission")]           // najviše 5 slanja / minuti
public async Task<IActionResult> Submit([FromBody] SubmitDto dto,
    [FromServices] IAntiforgery antiforgery)
{
    await antiforgery.ValidateRequestAsync(HttpContext);            // CSRF provjera

    var userId = int.Parse(User.FindFirstValue("webide:userId"));   // tko šalje
    // validacije: kod nije prazan, jezik je dozvoljen, problem postoji …

    var submission = await _repo.CreateAsync(userId, dto.ProblemId, // 1) redak u bazi
                                             dto.Language, dto.SourceCode); //    status=Pending

    var job = new { submission.Id, dto.ProblemId, dto.Language,     // 2) opis posla
                    dto.SourceCode, problem.TimeLimitMs, problem.MemoryLimitKb };

    await _redis.GetDatabase().ListRightPushAsync(                  // 3) u RED (RPUSH)
        "submissions:queue", JsonSerializer.Serialize(job));

    return Ok(new { submissionId = submission.Id });                // 4) vrati id pregledniku
}
```

**Ključno:** kontroler **ne izvršava kod**. Samo zapiše `Submission` (status
`Pending`), gurne posao u Redis red i **odmah** vrati `submissionId`. Preglednik ne
čeka — asinkrono je. Ovo je **producer** u obrascu *producer/consumer*.

Zašto Redis red? Da web app i worker budu razdvojeni. Ako naraste više workera,
svi vade iz istog reda i posao se raspodijeli.

### Korak 3 — Preglednik se pretplati na rezultate preko SignalR-a

`SignalR` je apstrakcija za dvosmjernu komunikaciju u realnom vremenu (WebSocket i
fallbackovi). Preglednik otvori vezu i pozove metodu na **hubu**:

```js
const conn = new signalR.HubConnectionBuilder().withUrl('/hubs/execution').build();
conn.on('submissionUpdate', (id, status, score, wall, mem) => { /* osvježi UI */ });
await conn.start();
await conn.invoke('SubscribeToSubmission', submissionId);
```

Hub (`WebIde.Frontend/Hubs/ExecutionHub.cs`) ubaci vezu u **grupu** vezanu uz taj
submission — i **provjeri da submission pripada tom korisniku**:

```csharp
[Authorize]
public class ExecutionHub(SubmissionRepository submissionRepo) : Hub
{
    public async Task SubscribeToSubmission(int submissionId)
    {
        var userId = int.Parse(Context.User!.FindFirst("webide:userId")!.Value);
        if (!await submissionRepo.IsOwnedByAsync(submissionId, userId))
            throw new HubException("forbidden");                  // ne smiješ pratiti tuđe
        await Groups.AddToGroupAsync(Context.ConnectionId, $"submission:{submissionId}");
    }
}
```

"Grupa" = skup veza kojima ćemo kasnije slati poruke. Ime grupe je
`submission:{id}`, pa svaki rezultat ide točno pravom pregledniku.

### Korak 4 — Worker vadi posao iz reda (`WebIde.Worker/Workers/SubmissionWorker.cs`)

Worker je `BackgroundService` koji se vrti u petlji i **vadi** iz istog reda (LPOP —
suprotni kraj od RPUSH, pa je red **FIFO**):

```csharp
while (!stoppingToken.IsCancellationRequested)
{
    var value = await db.ListLeftPopAsync("submissions:queue");    // vadi posao
    if (value.IsNull) { await Task.Delay(250); continue; }         // prazan red → čekaj
    var job = JsonSerializer.Deserialize<SubmissionJob>(value!);
    await ProcessJobAsync(job, stoppingToken);
}
```

`ProcessJobAsync` (isti file):

```csharp
// 1) učitaj Problem + test primjere iz baze
// 2) status = Running i JAVI pregledniku odmah:
await UpdateStatusAsync(factory, job.SubmissionId, SubmissionStatus.Running);
await PublishStatusAsync(job.SubmissionId, SubmissionStatus.Running);

// 3) POKRENI kod u sandboxu
var run = await orchestrator.RunAsync(job, problem, testCases, ct);

// 4) OCIJENI izlaz
var result = evaluator.Evaluate(run, testCases);

// 5) SPREMI rezultat u bazu
await PersistResultAsync(factory, job.SubmissionId, result, testCases);

// 6) OBJAVI konačni rezultat na Redis kanal (za SignalR most)
await redis.GetSubscriber().PublishAsync(
    RedisChannel.Literal($"execution:{job.SubmissionId}"), JsonSerializer.Serialize(evt));
```

### Korak 5 — Sandbox (`WebIde.Worker/Services/SandboxOrchestrator.cs`)

Za svaki submission worker preko Docker API-ja podigne **novi kontejner** s
odgovarajućom slikom (`sandbox-gcc` za C/C++, `sandbox-python`, `sandbox-node`).
Bitne su **sigurnosne postavke** (moraš ih znati nabrojati):

```csharp
HostConfig = new HostConfig
{
    NetworkMode    = "none",        // BEZ mreže
    ReadonlyRootfs = true,          // korijenski FS samo za čitanje
    Memory = memBytes, MemorySwap = memBytes,   // memorijsko ograničenje
    NanoCPUs = ..., PidsLimit = 64,             // CPU + ograničen broj procesa (anti fork-bomb)
    CapDrop = new[] { "ALL" },                  // ukloni sve Linux capabilities
    SecurityOpt = _securityOpts,                // no-new-privileges + seccomp (+ apparmor)
    Mounts = {
        { srcDir → /code  (ReadOnly = true)  }, // kod i test primjeri, SAMO čitanje
        { workDir → /work (ReadOnly = false) }, // ovdje se piše prevedeni binarij
    }
}
```

Wrapper skripta u slici (`sandbox/compile-and-run.sh`) prevede kod, pokrene ga
za **svaki test primjer** s vremenskim ograničenjem (`timeout`), usporedi izlaz s
očekivanim i **ispiše JSON s rezultatima po primjeru** na stdout. Orchestrator
pričeka kontejner, pokupi stdout/stderr/exit code i vrati ih.

> Detalj iz ovog projekta koji je vrijedno spomenuti: binarij se izvršava iz
> `/work` bind-mounta (ne iz `/tmp`) jer je `/tmp` u kontejneru `noexec`.

### Korak 6 — Ocjenjivanje (`WebIde.Worker/Services/SubmissionEvaluator.cs`)

Evaluator pretvori sirovi izlaz sandboxa u konačnu ocjenu:

```csharp
if (run.ExitCode == 2)  return CompileError;                    // greška kompajlera
if (run.ExitCode != 0 && run.ExitCode != 124) return InternalError(...);
var cases = JsonSerializer.Deserialize<List<SandboxCaseResult>>(run.Stdout);
// zbroji bodove po primjeru, odredi najgori verdikt (Accepted / WrongAnswer /
// TimeLimitExceeded / MemoryLimitExceeded / RuntimeError …) i ukupni Score
```

Verdikt je **najgori** rezultat po test primjerima (prioritet: MLE > TLE >
RuntimeError > WrongAnswer > Accepted).

### Korak 7 — Rezultat se sprema i objavljuje

`PersistResultAsync` upiše **jedan `ExecutionResult` po test primjeru** (stdout,
stderr, vrijeme, memorija, verdikt) i osvježi sam `Submission` (status, score…).
Zatim worker **PUBLISH-a** `SubmissionResultEvent` na kanal `execution:{id}`.

### Korak 8 — Most Redis → SignalR (`WebIde.Frontend/Services/RedisSubscriptionService.cs`)

Ovo je **jedina spona** koja nosi rezultat iz workera natrag u preglednik. To je
`BackgroundService` u web aplikaciji koji je pretplaćen na `execution:*`:

```csharp
await subscriber.SubscribeAsync(RedisChannel.Pattern("execution:*"),
    (channel, message) => _ = HandleMessageAsync(channel, message));

// za svaku poruku:
var evt = JsonSerializer.Deserialize<SubmissionResultEvent>(message);
await hub.Clients.Group($"submission:{evt.SubmissionId}")
    .SendAsync("submissionUpdate", evt.SubmissionId, evt.Status,
               evt.Score, evt.WallTimeMs, evt.PeakMemoryKb);
```

Zašto je potreban **most**? Jer worker i web app su **različiti procesi** — worker
nema SignalR veze prema pregledniku. Zato worker javi rezultat preko Redisa, a web
app (koji *drži* SignalR veze) proslijedi ga u pravu grupu.

### Korak 9 — Preglednik prikaže rezultat

`conn.on('submissionUpdate', …)` iz Koraka 3 se okine, UI pokaže npr.
`ACCEPTED — SCORE 100 — 12ms — 2048KB`. Kod konačnog verdikta stranica se osvježi
da pokaže puni stdout/stderr iz baze.

---

## 4. Ključni koncepti (kratko, za "zašto")

| Pojam | Objašnjenje u jednoj rečenici |
|---|---|
| **Producer/Consumer + Redis red** | Kontroler samo stavi posao u red; worker(i) ga vade — razdvaja primanje od izvršavanja i omogućuje skaliranje. |
| **Redis pub/sub** | Kanal `execution:{id}` na koji worker "objavi" rezultat, a web app sluša — jednosmjerna obavijest između procesa. |
| **SignalR hub + grupe** | Realtime kanal prema pregledniku; grupa `submission:{id}` cilja točno onog korisnika koji čeka. |
| **Sandbox izolacija** | `network=none`, read-only FS, cap-drop ALL, seccomp, memorijsko/CPU/pids ograničenje — da tuđi kod ne našteti hostu. |
| **Antiforgery (CSRF)** | Token uz svaki POST da netko drugi ne može poslati zahtjev u tvoje ime. |
| **Rate limiting** | `EnableRateLimiting("submission")` — max 5 slanja/min, protiv zloporabe. |
| **DataProtection na Redisu** | Ključevi za enkripciju cookieja su na Redisu da deploy ne odjavi sve korisnike. |
| **Dvije auth sheme** | GitHub OAuth (cookie) *i* Identity (korisničko ime/lozinka) — zato akcije koriste `Cookies,Identity.Application`. |

---

## 5. Moguća pitanja i kratki odgovori

- **"Gdje se izvršava korisnikov kod?"** — U zasebnom Docker kontejneru koji podiže
  `WebIde.Worker`, ne u web aplikaciji. Web app kod nikad ne pokreće.
- **"Kako rezultat dođe uživo do preglednika?"** — Worker objavi na Redis kanal →
  `RedisSubscriptionService` u web appu to proslijedi kroz SignalR grupu →
  preglednikov `submissionUpdate` handler osvježi UI.
- **"Što ako je više slanja odjednom?"** — Sva idu u isti Redis red; worker(i) ih
  vade FIFO redom (`RPUSH`/`LPOP`). `MaxConcurrentSandboxes` ograničava paralelizam.
- **"Zašto kontroler ne čeka rezultat?"** — Izvršavanje traje (kompajliranje +
  pokretanje). Sinkrono čekanje bi blokiralo request. Zato vraća `submissionId`
  odmah, a rezultat stiže naknadno preko SignalR-a.
- **"Kako sprječavate da korisnik prati tuđi submission?"** —
  `ExecutionHub.SubscribeToSubmission` provjeri `IsOwnedByAsync(submissionId, userId)`.
- **"Čemu služi `[FromBody]`?"** — Kaže ASP.NET-u da parametar `dto` popuni iz JSON
  tijela zahtjeva (za razliku od `[FromRoute]`/`[FromQuery]`).

---

## 6. Datoteke koje treba moći pokazati

| Tema | Datoteka |
|---|---|
| Slanje koda (kontroler) | `WebIde.Frontend/Controllers/SubmissionController.cs` |
| Primjer CRUD kontrolera | `WebIde.Frontend/Controllers/ProblemController.cs` |
| SignalR hub | `WebIde.Frontend/Hubs/ExecutionHub.cs` |
| Most Redis→SignalR | `WebIde.Frontend/Services/RedisSubscriptionService.cs` |
| Worker (petlja + tok) | `WebIde.Worker/Workers/SubmissionWorker.cs` |
| Docker sandbox | `WebIde.Worker/Services/SandboxOrchestrator.cs` |
| Ocjenjivanje | `WebIde.Worker/Services/SubmissionEvaluator.cs` |
| Frontend JS (Submit + SignalR) | `WebIde.Frontend/Views/Problem/Details.cshtml` (dio `@section Scripts`) |
| DI, middleware, auth, SignalR, rate limiter | `WebIde.Frontend/Program.cs` |
