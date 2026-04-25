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

            // 1. Seed Users
            if (!context.Users.Any())
            {
                var users = new[]
                {
                    new User(Guid.NewGuid(), "peter.gerardus@gmail.com", "Peter", "Gerardus", BCrypt.Net.BCrypt.HashPassword("PeterGerardus123!"), UserRole.Admin, UserStatus.Active),
                    new User(Guid.NewGuid(), "monique.degraaf@gmail.com", "Monique", "de Graaf", BCrypt.Net.BCrypt.HashPassword("MoniqueDeGraaf123!"), UserRole.ContentCreator, UserStatus.Active),
                    new User(Guid.NewGuid(), "dieter.gieter@gmail.com", "Dieter", "Gieter", BCrypt.Net.BCrypt.HashPassword("DieterGieter123!"), UserRole.User, UserStatus.Active)
                };
                context.Users.AddRange(users);
                context.SaveChanges();
            }

            // 2. Seed 5 Categories
            if (!context.Categories.Any())
            {
                var categories = new[]
                {
                    new Category("Artificial Intelligence", "ai", "Ontwikkelingen in machine learning en neurale netwerken."),
                    new Category("Web Development", "web-dev", "De nieuwste frameworks en frontend technieken."),
                    // Gebruik deze namen om de blogs te linken
                    new Category("Lifestyle & Productiviteit", "lifestyle", "Tips voor een gebalanceerd en efficiënt leven."),
                    new Category("Cybersecurity", "security", "Bescherming in de digitale wereld."),
                    new Category("Duurzame Tech", "duurzaamheid", "Groene energie en milieuvriendelijke innovaties.")
                };
                context.Categories.AddRange(categories);
                context.SaveChanges();
            }

            // 3. Seed 20 Articles
            if (!context.Articles.Any())
            {
                var admin = context.Users.First(u => u.Email == "peter.gerardus@gmail.com");
                var creator = context.Users.First(u => u.Email == "monique.degraaf@gmail.com");
                var categories = context.Categories.ToList();

                var blogData = new List<(string Title, string Slug, string Summary, string Content, string CatSlug, Guid AuthorId)>
                {
                    // AI Category
                    ("De opkomst van Generatieve AI", "opkomst-generatieve-ai", "Hoe AI onze creativiteit verandert.", "Generatieve AI zoals ChatGPT en Midjourney hebben de wereld in korte tijd veranderd. In dit artikel kijken we naar de impact op de arbeidsmarkt en hoe we deze tools ethisch kunnen inzetten. De komende jaren zal de integratie van AI in dagelijkse software alleen maar toenemen...", "ai", admin.Id),
                    ("AI in de Zorg", "ai-in-de-zorg", "Revolutie in diagnoses.", "Medische professionals gebruiken steeds vaker AI om sneller diagnoses te stellen. Van het herkennen van tumoren op scans tot het voorspellen van epidemieën. De precisie van machine learning modellen overstijgt in sommige gevallen zelfs het menselijk oog.", "ai", creator.Id),
                    ("De Ethiek van Robots", "ethiek-van-robots", "Wie is verantwoordelijk?", "Als een zelfrijdende auto een ongeluk veroorzaakt, wie is dan de schuldige? De discussie over AI-wetgeving is in volle gang binnen de Europese Unie. Transparantie en menselijke controle blijven de belangrijkste pijlers in dit debat.", "ai", admin.Id),
                    ("Deepfakes: De schaduwkant", "deepfakes-schaduwkant", "Gevaar van desinformatie.", "Niet alles wat je ziet is echt. Deepfakes maken het mogelijk om mensen dingen te laten zeggen die ze nooit hebben gezegd. Hoe wapenen we ons tegen deze vorm van digitale manipulatie?", "ai", creator.Id),

                    // Web Dev Category
                    ("React vs Vue in 2026", "react-vs-vue-2026", "Welk framework wint de strijd?", "De frontend wereld staat nooit stil. Terwijl React dominant blijft dankzij de enorme community, wint Vue terrein door zijn eenvoud en snelheid. In dit artikel vergelijken we de nieuwste features van beide grootmachten.", "web-dev", creator.Id),
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
                    var catId = categories.First(c => c.Slug == data.CatSlug).Id;
                    var article = new Article(data.Title, data.Slug, data.Summary, data.Content, data.AuthorId, catId);

                    // Direct publiceren
                    article.Publish();
                    context.Articles.Add(article);
                }

                context.SaveChanges();
            }
        }
    }
}