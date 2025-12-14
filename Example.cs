using LetterboxdFeed;
using LetterboxdFeed.Options;

// Configuration
var options = new LetterboxdFeedOptions
{
    PollingInterval = 5, // Vérifier toutes les 5 minutes
    EnableAutoPolling = true
};

// Créer l'instance
using var feed = new LetterboxdFeed(options);

// Ajouter des utilisateurs à surveiller
feed.AddUsername("username1");
feed.AddUsername("username2");
// Ou ajouter plusieurs utilisateurs en une fois
// feed.AddUsernames(new[] { "username1", "username2", "username3" });

// Événement déclenché quand un utilisateur regarde un nouveau film
feed.MovieWatched += (sender, args) =>
{
    Console.WriteLine($"🎬 {args.Username} a regardé : {args.Movie.Title} ({args.Movie.FilmYear})");
    
    if (args.Movie.Rate > 0)
    {
        Console.WriteLine($"⭐ Note : {args.Movie.Rate}/5");
    }
    
    if (args.Movie.IsARewatch)
    {
        Console.WriteLine("🔄 Rewatch");
    }
    
    if (!string.IsNullOrEmpty(args.Movie.Review))
    {
        Console.WriteLine($"💭 Avis : {args.Movie.Review}");
    }
    
    Console.WriteLine($"🔗 Lien : {args.Movie.Link}");
    Console.WriteLine();
};

// Événement déclenché en cas d'erreur
feed.ErrorOccurred += (sender, args) =>
{
    Console.WriteLine($"❌ Erreur pour {args.Username} : {args.Exception.Message}");
};

// Événements de polling
feed.PollingStarted += (sender, args) =>
{
    Console.WriteLine("✅ Surveillance démarrée");
};

feed.PollingStopped += (sender, args) =>
{
    Console.WriteLine("⏸️ Surveillance arrêtée");
};

// Démarrer la surveillance
feed.StartPolling();

Console.WriteLine("📺 Surveillance des flux Letterboxd en cours...");
Console.WriteLine("Appuyez sur Entrée pour arrêter...");
Console.ReadLine();

// Arrêter la surveillance
feed.StopPolling();

