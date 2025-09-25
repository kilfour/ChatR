[↩ Back](./readme.md) 
# **ChatR**

## Deliverables 

1. **Projectreadme** (Markdown): korte beschrijving per stap, keuzes en valkuilen.
2. **Screenshots** of korte screen capture van elke stap in actie (DevTools Network waar relevant).
3. **Configuratie‑notities**: host/URL, CORS, proxy‑instellingen (indien van toepassing).
4. **Meetresultaten**: payloadvergelijking JSON vs MessagePack, plus korte interpretatie.
5. **Risico‑/securitychecklist** ingevuld (zie onderaan).

> Tip: structureer je repo in mappen per increment (`/step-01`, …) óf documenteer duidelijke tags/branches.


## Functioneliteit per increment

### 1. **Hello Hub**

* Een werkende hub en minimale webclient.
* Wanneer gebruiker A een bericht stuurt, verschijnt het bij álle verbonden clients.
* **Niet‑functionele eisen:** stabiele verbinding, foutmeldingen zichtbaar in UI‑log of console.
* **Testen:** controleer gelijktijdig in twee browservensters.

### 2. **Rooms**

* Gebruikers kunnen één of meerdere **rooms** betreden en verlaten.
* Berichten worden alleen ontvangen door clients in dezelfde room.
* **Randgevallen:** room bestaat nog niet / is leeg, dubbele join is idempotent, leave zonder membership faalt niet hard.
* **Testen:** drie vensters: twee in room "alpha", één in "beta", berichten scheiden correct.

### 3. **Auth **

* Ten minste één hubmethode vereist **ingelogde** gebruiker.
* **Anonieme** gebruikers mogen **alleen lezen** (berichten ontvangen) maar niet verzenden.
* **Security‑notities:** beschrijf je gekozen auth‑mechanisme (cookie/JWT), CORS‑policy en SameSite‑overwegingen.
* **Testen:** anonieme sessie vs ingelogde sessie, verzendrechten verschillen.

### 4. **Progress UI (Lange taak)**

* Een serveractie simuleert een **langdurige taak** (bijv. 5–10 seconden) en pusht **progress‑events** (bijv. 0%, 25%, 50%, 75%, 100%).
* **UI‑vereiste:** voortgang zichtbaar (tekstueel of indicator), eindstatus duidelijk.
* **Betrouwbaarheid:** progress loopt door bij één client, andere clients mogen optioneel meekijken.
* **Testen:** start taak, observeer meerdere progress‑events in tijd.

### 5. **MessagePack + Meting**
* Schakel **MessagePack** in als alternatief protocol.
* **Meetopzet:** stuur representatieve berichten (minstens 50 met variërende grootte) en **noteer payloadgrootte** met DevTools Network.
* **Rapportage:** tabel met gemiddelde/grootste/kleinste payload (JSON vs MessagePack) + korte analyse (winsten, trade‑offs, compatibiliteit).

### 6. **Reconnect UX**

* De client toont een **duidelijke status** bij verbindingsverlies (bijv. banner of statuslabel) en **disabled** verzenden totdat de verbinding hersteld is.
* Bij **herstel** verdwijnt de banner en is verzenden weer mogelijk.
* **Randgevallen:** meerdere reconnectpogingen, mislukt herstel geeft bruikbare feedback.
* **Testen:** simuleer netwerkverlies (DevTools => Offline) en controleer UI‑gedrag.

## Acceptatiecriteria (per stap)

* **Correctheid:** functionaliteit werkt zoals beschreven, inclusief randgevallen.
* **Traceerbaarheid:** je Readme beschrijft wat je hebt gebouwd, hoe je het testte en wat je leerde.
* **Observeerbaarheid:** er is zichtbare logging/telemetrie (console/DevTools) voor negotiate/transport en fouten.
* **Veiligheid:** auth‑pad is consistent, anonieme vs geauthenticeerde rechten zijn duidelijk gescheiden.
* **UX:** feedback bij wachten, fouten en reconnect is begrijpelijk en niet opdringerig.


## Troubleshooting checklist

* **/negotiate** status ≠ 200 => controleer pad, CORS, auth, HTTPS/mixed content.
* **WebSocket upgrade** geblokkeerd => kijk naar proxy/LB‑headers, val terug op SSE/Long Polling.
* **401/403** op negotiate => cookies/SameSite of JWT via query/token‑factory.
* **404** op hub‑pad => map‑route versus client‑URL, base path/omschrijvingen bij reverse proxy.
* **Meerdere instances** => voor deze oefening uit scope, noteer wel hoe je het zou schalen (Redis/Azure SignalR).

## Security checklist

* [ ] Hub/methode‑autorisatie sluit aan op businessregels.
* [ ] Alleen noodzakelijke origins toegestaan (CORS).
* [ ] Invoer gevalideerd (max. lengte, contentregels) voordat je broadcast.
* [ ] Logging zonder gevoelige gegevens.
* [ ] Duidelijke limieten/rate‑limit (desnoods eenvoudig, beschrijf je aanpak).


## Inlevering

* Repo‑link met duidelijke instructies om lokaal te runnen.
* Readme met stappen, metingen en screenshots.
* Korte reflectie (+/- halve pagina): wat werkte meteen, wat kostte tijd, wat zou je anders doen in productie?
