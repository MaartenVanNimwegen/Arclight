using Arclight.Domain.Entities;
using Arclight.Domain.Enums;
using Arclight.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Arclight.Infrastructure.Data
{
    public static class DbInitializer
    {
        public static void Initialize(AppDbContext context)
        {
            context.Database.Migrate();

            var seedUsers = new List<(string Email, string FirstName, string LastName, string Password, UserRole Role)>
            {
                ("peter.gerardus@gmail.com", "Peter", "Gerardus", "PeterGerardus123!", UserRole.Admin),
                ("monique.degraaf@gmail.com", "Monique", "de Graaf", "MoniqueDeGraaf123!", UserRole.ContentCreator),
                ("dieter.gieter@gmail.com", "Dieter", "Gieter", "DieterGieter123!", UserRole.User)
            };

            foreach (var u in seedUsers)
            {
                if (!context.Users.Any(user => user.Email == u.Email))
                {
                    context.Users.Add(new User(
                        Guid.NewGuid(),
                        u.Email,
                        u.FirstName,
                        u.LastName,
                        BCrypt.Net.BCrypt.HashPassword(u.Password),
                        u.Role,
                        UserStatus.Active));
                }
            }
            context.SaveChanges();

            var seedCategories = new List<(string Name, string Slug, string Description)>
            {
                ("Artificial Intelligence", "ai", "Ontwikkelingen in machine learning en neurale netwerken."),
                ("Web Development", "web-dev", "De nieuwste frameworks en frontend technieken."),
                ("Lifestyle & Productiviteit", "lifestyle", "Tips voor een gebalanceerd en efficiënt leven."),
                ("Cybersecurity", "security", "Bescherming in de digitale wereld."),
                ("Duurzame Tech", "duurzaamheid", "Groene energie en milieuvriendelijke innovaties.")
            };

            foreach (var c in seedCategories)
            {
                if (!context.Categories.Any(cat => cat.Slug == c.Slug))
                {
                    context.Categories.Add(new Category(c.Name, c.Slug, c.Description));
                }
            }
            context.SaveChanges();

            if (!context.Articles.Any())
            {
                var admin = context.Users.FirstOrDefault(u => u.Email == "peter.gerardus@gmail.com");
                var creator = context.Users.FirstOrDefault(u => u.Email == "monique.degraaf@gmail.com");
                var categories = context.Categories.ToList();

                if (admin == null || creator == null) return;

                var blogData = new List<(string Title, string Slug, string Summary, string Content, string CatSlug, Guid AuthorId)>
                {
                    // AI Category
                    ("De opkomst van Generatieve AI", "opkomst-generatieve-ai", "Hoe AI onze creativiteit verandert.", "Generatieve AI zoals ChatGPT, Midjourney en Claude hebben de wereld in een recordtempo veranderd. Waar we voorheen spraken over algoritmes die data analyseerden, praten we nu over systemen die nieuwe realiteiten creëren. In dit artikel duiken we diep in de impact op de arbeidsmarkt en de ethische dilemma's van 2026.\r\n\r\nDe Verschuiving van Gereedschap naar Partner\r\nIn de begindagen van AI zagen we het vooral als een geavanceerde zoekmachine. Vandaag de dag fungeert AI als een 'co-pilot' in bijna elke sector. Voor programmeurs schrijft het de boilerplate code; voor ontwerpers genereert het de eerste concepten op basis van ruwe schetsen. Deze verschuiving betekent dat de vaardigheid 'prompt engineering' is geëvolueerd naar 'AI-collaboratie'. Het gaat niet meer om de vraag of AI je werk overneemt, maar hoe jij AI gebruikt om je output te verdrievoudigen.\r\n\r\nDe Arbeidsmarkt in Transitie\r\nWe zien een paradoxale beweging op de arbeidsmarkt. Terwijl repetitieve taken verdwijnen, ontstaat er een enorme vraag naar mensen die de output van AI kunnen valideren en verfijnen. 'Curatie' wordt belangrijker dan 'creatie'. De ethische vragen blijven echter prangend. Wie bezit de rechten op een tekst die door een model is gegenereerd dat getraind is op miljarden menselijke teksten? De komende jaren zal de wetgeving (zoals de EU AI Act) de kaders scheppen waarbinnen we deze technologie veilig kunnen integreren zonder de menselijke autonomie te verliezen.", "ai", admin.Id),
                    ("AI in de Zorg", "ai-in-de-zorg", "Revolutie in diagnoses.", "De medische wereld bevindt zich in de grootste revolutie sinds de uitvinding van de antibiotica. Medische professionals gebruiken steeds vaker AI om sneller en accurater diagnoses te stellen.\r\n\r\nPrecisie-Geneeskunde\r\nVan het herkennen van minuscule afwijkingen op MRI-scans tot het voorspellen van zeldzame genetische aandoeningen: de precisie van moderne machine learning modellen overstijgt in sommige gevallen zelfs het menselijk oog van de meest ervaren radioloog. Dit betekent niet dat de arts overbodig wordt, maar dat de arts wordt uitgerust met een 'superkracht'. AI kan duizenden wetenschappelijke papers in enkele seconden scannen om een behandelplan op maat te maken voor een specifieke patiënt.\r\n\r\nVoorspellen van Epidemieën\r\nNaast individuele zorg speelt AI een cruciale rol in de volksgezondheid. Door patronen in ziekenhuisopnames en zelfs zoekopdrachten op internet te analyseren, kunnen AI-systemen een uitbraak van griep of andere virussen voorspellen voordat deze zich grootschalig verspreidt. De uitdaging ligt hier bij privacy. Hoe delen we medische data om levens te redden zonder de anonimiteit van het individu te schenden? In 2026 is 'federated learning' – waarbij modellen getraind worden op lokale data zonder deze te kopiëren – de gouden standaard geworden om deze balans te vinden.", "ai", creator.Id),
                    ("De Ethiek van Robots", "ethiek-van-robots", "Wie is verantwoordelijk?", "Als een zelfrijdende auto een ongeluk veroorzaakt, wie is dan de schuldige? De fabrikant, de programmeur, of de eigenaar van de auto? Naarmate robots fysiek actiever worden in onze openbare ruimte, worden deze vragen van levensbelang.\r\n\r\nHet Trolley-probleem in de Praktijk\r\nDe discussie over AI-wetgeving is in volle gang binnen de Europese Unie. Het is niet langer een theoretisch 'trolley-probleem'. Ingenieurs moeten nu beslissen welke prioriteiten een algoritme stelt in een noodsituatie. De roep om transparantie (Explainable AI) is luider dan ooit. We kunnen het ons niet veroorloven dat systemen beslissingen nemen op basis van een 'black box'.\r\n\r\nMenselijke Controle en Verantwoordelijkheid\r\nDe consensus in 2026 is dat er altijd een 'human-in-the-loop' moet zijn bij kritieke beslissingen. Of het nu gaat om militaire toepassingen of zorgrobots die ouderen helpen: de menselijke controle blijft de belangrijkste pijler. We moeten voorkomen dat we verantwoordelijkheid delegeren aan machines die geen moreel kompas hebben. Ethiek moet ingebakken zitten in het designproces (Ethics by Design) in plaats van een pleister achteraf te zijn.", "ai", admin.Id),
                    ("Deepfakes: De schaduwkant", "deepfakes-schaduwkant", "Gevaar van desinformatie.", "Niet alles wat je ziet of hoort is nog de waarheid. Deepfakes maken het mogelijk om mensen dingen te laten zeggen en doen die ze nooit hebben gedaan, met een realisme dat nauwelijks van echt te onderscheiden is.\r\n\r\nDe Erosie van Vertrouwen\r\nHet grootste gevaar van deepfakes is niet alleen de desinformatie zelf, maar de 'liar’s dividend': het fenomeen waarbij echte beelden worden afgedaan als nep, omdat 'alles toch gemanipuleerd kan zijn'. Dit ondermijnt de journalistiek en het rechtssysteem. We zien een wapenwedloop tussen de makers van deepfakes en de ontwikkelaars van detectiesoftware. Blockchain-technologie wordt nu ingezet om de herkomst (provenance) van media te verifiëren, zodat we met zekerheid kunnen zeggen: dit beeld is rechtstreeks van deze camera gekomen.\r\n\r\nDigitale Geletterdheid\r\nHoe wapenen we ons tegen deze vorm van digitale manipulatie? Technologie alleen is niet genoeg. Het onderwijs speelt een cruciale rol. Burgers moeten leren om bronnen kritisch te beoordelen en de 'technische imperfecties' van oudere deepfakes te herkennen, al worden die steeds zeldzamer. In een wereld waar de waarheid vloeibaar is, is kritisch denken onze belangrijkste verdedigingslinie.", "ai", creator.Id),

                    // Web Dev Category
                    ("React vs Vue in 2026", "react-vs-vue-2026", "Welk framework wint de strijd?", "De frontend wereld staat nooit stil. In 2026 is de strijd tussen de twee giganten, React en Vue, heviger dan ooit. Terwijl React dominant blijft dankzij de enorme community en de steun van Meta, heeft Vue een trouwe achterban behouden door zijn ongeëvenaarde eenvoud en de introductie van revolutionaire reactivity-modellen.\r\n\r\nReact: De Kracht van Server Components\r\nReact heeft de afgelopen jaren zwaar ingezet op Server Components. De scheiding tussen client en server is bijna volledig vervaagd, waardoor applicaties sneller laden dan ooit tevoren. De leercurve is echter gestegen; het ecosysteem is complexer geworden. Developer Experience (DX) staat centraal, maar de overhead van 'het juiste pad' kiezen binnen React kan voor beginners ontmoedigend zijn.\r\n\r\nVue: Elegantie en Performance\r\nVue daarentegen blijft winnen op het gebied van toegankelijkheid. Met de komst van Vue 4 (hypothetisch voor 2026) is de compiler-geoptimaliseerde aanpak de standaard geworden. Bijna alle 'reactivity' wordt tijdens de build-time afgehandeld, wat resulteert in een extreem kleine bundle-size. Voor teams die snelheid van ontwikkeling en eenvoudige onboarding prioriteren, blijft Vue de favoriet. In dit artikel vergelijken we de nieuwste benchmarks en kijken we welk framework het meest toekomstbestendig is voor jouw volgende project.", "web-dev", creator.Id),
                    ("Tailwind CSS: Waarom je het moet gebruiken", "waarom-tailwind-css", "Styling was nog nooit zo makkelijk.", "Tailwind CSS heeft de manier waarop we styling schrijven getransformeerd. Geen gedoe meer met enorme CSS-bestanden, maar snelle utility classes direct in je HTML. Het bevordert een consistente design-taal door je hele applicatie.", "web-dev", admin.Id),
                    ("De terugkeer van SSR", "terugkeer-van-ssr", "Server Side Rendering is terug.", "Waar we eerst alles naar de client verschoven, zien we nu een beweging terug naar de server. Frameworks zoals Next.js en Remix laten zien dat SSR essentieel is voor SEO en performance op mobiele apparaten.", "web-dev", creator.Id),
                    ("WebAssembly: De toekomst?", "webassembly-toekomst", "Draai C++ in de browser.", "WebAssembly maakt het mogelijk om high-performance code te draaien in de webomgeving. Dit opent deuren voor complexe video-editing tools en games die voorheen onmogelijk waren in een browser.", "web-dev", admin.Id),

                    // Lifestyle Category
                    ("De 5 AM Club", "5-am-club-ervaring", "Vroeg opstaan voor succes.", "Veel succesvolle ondernemers zweren bij vroeg opstaan. Ik probeerde het een maand lang en de resultaten waren verrassend. Meer focus, minder afleiding en een voorsprong op de dag.", "lifestyle", admin.Id),
                    ("Digitaal Minimalisme", "digitaal-minimalisme", "Minder schermtijd, meer leven.", "Onze telefoons slokken uren van onze tijd op. Door kritisch te kijken naar welke apps we echt nodig hebben, creëren we rust in ons hoofd. Stop met scrollen en begin met leven.", "lifestyle", creator.Id),
                    ("Gezond Werken in de IT", "gezond-werken-it", "Voorkom een burn-out.", "Lange dagen achter een scherm eisen hun tol. In dit artikel delen we tips over ergonomie, pauzes en het belang van beweging voor software developers.", "lifestyle", creator.Id),
                    ("De kracht van Meditatie", "kracht-van-meditatie", "Rust in een drukke wereld.", "Slechts 10 minuten per dag mediteren kan je stressniveau drastisch verlagen. We bespreken verschillende technieken voor beginners en gevorderden.", "lifestyle", admin.Id),

                    // Security Category
                    ("Wachtwoorden zijn verleden tijd", "wachtwoorden-verleden-tijd", "De overstap naar Passkeys.", "Passkeys zijn veiliger en makkelijker dan traditionele wachtwoorden. Grote tech-reuzen zoals Google en Apple pushen deze nieuwe standaard om phishing tegen te gaan.", "security", admin.Id),
                    ("Zero Trust Architectuur", "zero-trust-uitleg", "Vertrouw niemand op je netwerk.", "In een wereld waar hackers steeds slimmer worden, is 'Zero Trust' de nieuwe standaard. Elke gebruiker en elk apparaat moet constant geverifieerd worden, ongeacht waar ze zich bevinden.", "security", creator.Id),
                    ("Social Engineering Gevaren", "social-engineering", "Hackers vissen naar je gegevens.", "De zwakste schakel in beveiliging is vaak de mens. Leer hoe je phishing, vishing en andere vormen van manipulatie kunt herkennen voordat het te laat is.", "security", admin.Id),
                    ("Privacy in het Web3 tijdperk", "privacy-web3", "Wie bezit jouw data?", "Web3 belooft een gedecentraliseerd internet waar jij de baas bent over je eigen gegevens. Maar hoe zit het met de privacy op een openbare blockchain?", "security", creator.Id),

                    // Duurzaamheid Category
                    ("Groene Datacenters", "groene-datacenters", "De ecologische voetafdruk van de cloud.", "Het internet verbruikt enorme hoeveelheden stroom. Gelukkig stappen steeds meer cloud-providers over op 100% hernieuwbare energie en innovatieve koelsystemen.", "duurzaamheid", admin.Id),
                    ("Elektrisch rijden: De feiten", "elektrisch-rijden-feiten", "Zijn EV's echt beter?", "We duiken in de data achter elektrische auto's. Van de productie van batterijen tot het recyclen van onderdelen. Wat is de werkelijke impact op het milieu?", "duurzaamheid", admin.Id),
                    ("Slimme thermostaten", "slimme-thermostaten", "Bespaar energie en geld.", "Een slimme thermostaat leert van je gedrag en verwarmt je huis alleen wanneer dat nodig is. Een kleine investering die zichzelf snel terugbetaalt voor het milieu en je portemonnee.", "duurzaamheid", creator.Id),
                    ("De impact van Fast Fashion", "impact-fast-fashion", "Kleding en milieu.", "De kledingindustrie is een van de meest vervuilende ter wereld. Door te kiezen voor kwaliteit boven kwantiteit kunnen we een groot verschil maken.", "duurzaamheid", creator.Id)
                };

                foreach (var data in blogData)
                {
                    var category = categories.FirstOrDefault(c => c.Slug == data.CatSlug);
                    if (category != null)
                    {
                        var article = new Article(data.Title, data.Slug, data.Summary, data.Content, data.AuthorId, category.Id);
                        article.Publish();
                        context.Articles.Add(article);
                    }
                }
                context.SaveChanges();
            }
        }
    }
}