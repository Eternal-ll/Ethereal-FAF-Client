using Ethereal.FAF.UI.Client.Infrastructure.Services.Interfaces;
using Ethereal.FAF.UI.Client.Models.Lobby;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Imaging;

namespace Ethereal.FAF.UI.Client.Infrastructure.Services
{
    internal class GameMapPreviewCacheService
    {
        private readonly IFafGamesEventsService _gamesEventsService;
        private readonly IBackgroundImageCacheService _backgroundImageCacheService;
        private readonly ConcurrentDictionary<string, GameMapPreviewStorage> _smallMapPreviews = new();

        public GameMapPreviewCacheService(IFafGamesEventsService gamesEventsService, IBackgroundImageCacheService backgroundImageCacheService)
        {
            _gamesEventsService = gamesEventsService;

            gamesEventsService.GameAdded += GamesEventsService_GameAdded;
            gamesEventsService.GameUpdated += GamesEventsService_GameUpdated;
            gamesEventsService.GameRemoved += GamesEventsService_GameRemoved;
            _backgroundImageCacheService = backgroundImageCacheService;
        }

        private void GamesEventsService_GameRemoved(object sender, Game e)
        {
            if (e.SmallMapPreview != null)
            {
                if (_smallMapPreviews.TryGetValue(e.SmallMapPreview, out var cache))
                {
                    cache.Detach(e);
                    if (cache.NoGamesAttaches())
                    {
                        _smallMapPreviews.Remove(e.SmallMapPreview, out _);
                        cache.Dispose();
                    }
                }
            }
        }

        private void GamesEventsService_GameUpdated(object sender, (Game Cached, Game Incoming) e)
        {
            if (e.Incoming.SmallMapPreview == null)
            {
                if (e.Cached.SmallMapPreview != null)
                {

                }
                return;
            }
        }

        private void GamesEventsService_GameAdded(object sender, Models.Lobby.Game e)
        {
            if (e.SmallMapPreview != null)
            {
                if (_smallMapPreviews.TryGetValue(e.SmallMapPreview, out var cached))
                {
                    cached.Attach(e);
                    if (cached.Loaded())
                    {
                        e.MapSmallBitmapImage = cached.BitmapImage;
                    }
                }
                else
                {
                    var cache = new GameMapPreviewStorage();
                    if (!_smallMapPreviews.TryAdd(e.SmallMapPreview, cache))
                    {
                        return;
                    }
                    cache.Attach(e);
                    _backgroundImageCacheService.Load(e.SmallMapPreview, x =>
                    {
                        BitmapImage image = new();
                        image.BeginInit();
                        image.DecodePixelWidth = 60;
                        image.DecodePixelHeight = 60;
                        image.CacheOption = BitmapCacheOption.OnLoad;
                        image.UriSource = new Uri(x);
                        image.EndInit();
                        image.Freeze();
                        if (_smallMapPreviews.TryGetValue(e.SmallMapPreview, out var cache))
                        {
                            if (!cache.Loaded())
                            {
                                cache.Load(image);
                            }
                            else
                            {

                            }
                        }
                    });
                }
            }
        }
        internal class GameMapPreviewStorage : IDisposable
        {
            private bool _loaded;
            private List<Game> _games = new();
            public BitmapImage BitmapImage { get; private set; }

            public bool NoGamesAttaches() => _games.Count == 0;
            public void Attach(Game game) => _games.Add(game);
            public void Detach(Game game)
            {
                var found = _games.FirstOrDefault(x => x.Uid == game.Uid);
                if (found != null) found.MapSmallBitmapImage = null;
                _games.Remove(found);
            }
            public void Load(BitmapImage image)
            {
                BitmapImage = image;
                _loaded = false;
                foreach (var game in _games)
                {
                    game.MapSmallBitmapImage = image;
                }
            }
            public bool Loaded() => _loaded;
            public void Dispose()
            {
                if (_loaded) return;
                if (BitmapImage == null) return;
                BitmapImage.UriSource = null;
            }
        }
    }
}
