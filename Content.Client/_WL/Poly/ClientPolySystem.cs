using Content.Client.Sprite;
using Content.Shared._WL.Poly.Events;
using JetBrains.Annotations;
using SixLabors.ImageSharp;
using System.IO;

namespace Content.Client._WL.Poly
{
    [UsedImplicitly]
    public sealed partial class ClientPolySystem : EntitySystem
    {
        [Dependency] private readonly ContentSpriteSystem _contentSpriteSystem = default!;
        [Dependency] private readonly ILogManager _log = default!;

        private ISawmill _sawmill = default!;

        public override void Initialize()
        {
            base.Initialize();

            _sawmill = _log.GetSawmill("poly");

            SubscribeNetworkEvent<PolyServerQueryEvent>(OnServerQuery);
        }

        private async void OnServerQuery(PolyServerQueryEvent args)
        {
            var ent = GetEntity(args.Entity);

            try
            {
                _sawmill.Error($"Да ну этот щиткод, я не буду это править!");

                //await _contentSpriteSystem.Export(ent, Direction.South, (queue, image) =>
                //await _contentSpriteSystem.Export(ent, Direction.South, true) =>
                //{
                //    try
                //    {
                //        //TODO: проверить захватывает ли GC потоки, кхм
                //        using var stream = new MemoryStream(1024 * 14);

                //        image.SaveAsPng(stream);

                //        stream.Position = 0;
                //        var bytes = stream.GetBuffer();

                //        var ev = new PolyClientResponseEvent(bytes, args.QueryId);

                //        _sawmill.Info($"Запрос от Поли успешно обработан! Сущность: {ToPrettyString(args.Entity)}");

                //        RaiseNetworkEvent(ev);
                //    }
                //    catch (Exception ex)
                //    {
                //        _sawmill.Error($"Неизвестная ошибка при рендере фотографии для Поли! {ex.Message}");
                //    }
                //};
            }
            catch (Exception exc)
            {
                _sawmill.Error($"Error: {exc.Message}");
            }
        }
    }
}
