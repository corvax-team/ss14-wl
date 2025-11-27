using Content.Client.Administration.Managers;
using Content.Shared.Chat.TypingIndicator;
using Content.Shared.Verbs;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Shared.ContentPack;
using Robust.Shared.Exceptions;
using Robust.Shared.Timing;
using Robust.Shared.Utility;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Collections.Concurrent;
using System.IO;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Color = Robust.Shared.Maths.Color;

namespace Content.Client.Sprite;

public sealed class ContentSpriteSystem : EntitySystem
{
    [Dependency] private readonly IClientAdminManager _adminManager = default!;
    [Dependency] private readonly IClyde _clyde = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IResourceManager _resManager = default!;
    [Dependency] private readonly IUserInterfaceManager _ui = default!;
    [Dependency] private readonly IRuntimeLog _runtimeLog = default!;
    //WL-Changes-start
    [Dependency] private readonly ILogManager _logMan = default!;
    [Dependency] private readonly AppearanceSystem _appearance = default!;
    [Dependency] private readonly SpriteSystem _sprite = default!;

    private ISawmill _sawmill = default!;
    //WL-Changes-end

    private ContentSpriteControl<Rgba32> _control = default!;

    public static readonly ResPath Exports = new ResPath("/Exports");

    public override void Initialize()
    {
        base.Initialize();

        //WL-Changes-start
        _sawmill = _logMan.GetSawmill("sprite.export");
        _control = new(_appearance);
        //WL-Changes-end

        _resManager.UserData.CreateDir(Exports);
        _ui.RootControl.AddChild(_control);
        SubscribeLocalEvent<GetVerbsEvent<Verb>>(GetVerbs);
    }

    public override void Shutdown()
    {
        base.Shutdown();

        //WL-Changes-start
        _control.CancelAllQueued();
        //WL-Changes-end

        _ui.RootControl.RemoveChild(_control);
    }

    /// <summary>
    /// Exports sprites for all four directions.
    /// </summary>
    public async Task Export(EntityUid entity, bool includeId = true, CancellationToken cancelToken = default)
    {
        var tasks = new Task[4];
        var i = 0;

        foreach (var dir in new Direction[]
                 {
                     Direction.South,
                     Direction.East,
                     Direction.North,
                     Direction.West,
                 })
        {
            tasks[i++] = Export(entity, dir, includeId: includeId, cancelToken);
        }

        await Task.WhenAll(tasks);
    }

    //WL-Changes-start
    /// <summary>
    /// Exports sprite for a direction using a custom callback.
    /// </summary>
    public async Task Export(
        EntityUid entity,
        Direction direction,
        Action<ContentSpriteControl<Rgba32>.QueueEntry, Image<Rgba32>> action,
        CancellationToken cancelToken = default)
    {
        if (!_timing.IsFirstTimePredicted)
            return;

        if (!TryComp(entity, out SpriteComponent? spriteComp))
            return;

        var size = Vector2i.Zero;

        foreach (var layer in spriteComp.AllLayers)
        {
            if (!layer.Visible)
                continue;

            size = Vector2i.ComponentMax(size, layer.PixelSize);
        }

        if (size.Equals(Vector2i.Zero))
            return;

        IRenderTexture texture;
        try
        {
            texture = _clyde.CreateRenderTarget(size, new RenderTargetFormatParameters(RenderTargetColorFormat.Rgba8Srgb), name: "export");
        }
        catch (Exception ex)
        {
            _sawmill.Error($"Failed to create render target for export: {ex}");
            return;
        }

        var tcs = new TaskCompletionSource<bool>();

        if (cancelToken.CanBeCanceled)
        {
            cancelToken.Register(() =>
            {
                if (tcs.TrySetCanceled(cancelToken))
                {
                    try
                    {
                        texture.Dispose();
                        _sawmill.Debug($"Export cancelled for entity {entity}, direction {direction} (texture disposed).");
                    }
                    catch { }
                }
            });
        }

        _control.Enqueue((texture, direction, entity, Vector2.One, tcs, action));

        await tcs.Task;
    }

    /// <summary>
    /// Exports sprite at default scale (<see cref="Vector2.One"/>) using custom callback.
    /// </summary>
    public async Task ExportWithDefaultScale(
        Entity<SpriteComponent?> entity,
        Direction dir,
        Action<ContentSpriteControl<Rgba32>.QueueEntry, Image<Rgba32>> action,
        CancellationToken cancelToken = default)
    {
        if (!Resolve(entity, ref entity.Comp, false))
            return;

        var prototype = MetaData(entity).EntityPrototype?.ID;
        if (prototype == null)
            return;

        var newEntity = Spawn(prototype);

        try
        {
            _sprite.CopySprite(entity, newEntity);
            _sprite.SetScale(newEntity, Vector2.One);

            await Export(newEntity, dir, action, cancelToken);
        }
        finally
        {
            TryQueueDel(newEntity);
        }
    }

    /// <summary>
    /// Exports sprite at default scale (<see cref="Vector2.One"/>) and saves result.
    /// </summary>
    public async Task ExportWithDefaultScale(
        EntityUid entity,
        Direction dir,
        bool includeId,
        CancellationToken cancelToken = default)
    {
        await ExportWithDefaultScale(entity, dir, (queued, image) =>
        {
            SaveToExportFolder(entity, queued, image, includeId);
        }, cancelToken);
    }

    /// <summary>
    /// Exports default-scale (<see cref="Vector2.One"/>) sprites for all directions.
    /// </summary>
    public async Task ExportWithDefaultScale(
        EntityUid entity,
        bool includeId = true,
        CancellationToken cancelToken = default)
    {
        var tasks = new Task[4];
        var i = 0;

        foreach (var dir in new Direction[]
                 {
                     Direction.South,
                     Direction.East,
                     Direction.North,
                     Direction.West,
                 })
        {
            tasks[i++] = ExportWithDefaultScale(entity, dir, includeId: includeId, cancelToken);
        }

        await Task.WhenAll(tasks);
    }

    /// <summary>
    /// Exports sprite for direction and saves result.
    /// </summary>
    public async Task Export(
        EntityUid entity,
        Direction direction,
        bool includeId = true,
        CancellationToken cancelToken = default)
    {
        await Export(entity, direction, (queued, image) =>
        {
            SaveToExportFolder(entity, queued, image, includeId);
        }, cancelToken);
    }

    /// <summary>
    /// Saves rendered sprite to <see cref="Exports"/> folder.
    /// </summary>
    private void SaveToExportFolder(EntityUid entity, ContentSpriteControl<Rgba32>.QueueEntry queued, Image<Rgba32> image, bool includeId)
    {
        string filename;
        if (TryComp(entity, out MetaDataComponent? meta))
            filename = meta.EntityName ?? entity.ToString();
        else
            filename = entity.ToString();

        ResPath fullFileName;

        if (includeId)
        {
            fullFileName = Exports / $"{filename}-{queued.Direction}-{entity}.png";
        }
        else
        {
            fullFileName = Exports / $"{filename}-{queued.Direction}.png";
        }

        if (_resManager.UserData.Exists(fullFileName))
        {
            _sawmill.Info($"Found existing file {fullFileName} to replace.");
            _resManager.UserData.Delete(fullFileName);
        }

        using var file =
            _resManager.UserData.Open(fullFileName, FileMode.CreateNew, FileAccess.Write,
                FileShare.None);

        image.SaveAsPng(file);
        _sawmill.Info($"Saved screenshot to {fullFileName}");
    }
    //WL-Changes-end

    private void GetVerbs(GetVerbsEvent<Verb> ev)
    {
        if (!_adminManager.IsAdmin())
            return;

        var target = ev.Target;
        Verb verb = new()
        {
            Text = Loc.GetString("export-entity-verb-get-data-text"),
            Category = VerbCategory.Debug,
            Act = async () =>
            {
                try
                {
                    await ExportWithDefaultScale(target);
                }
                catch (Exception e)
                {
                    _runtimeLog.LogException(e, $"{nameof(ContentSpriteSystem)}.{nameof(Export)}");
                }
            },
        };

        ev.Verbs.Add(verb);
    }

    //WL-Changes-start
    public sealed class ContentSpriteControl<T> : Control where T : unmanaged, IPixel<T>
    {
        [Dependency] private readonly ILogManager _logMan = default!;
        [Dependency] private readonly IGameTiming _timing = default!;

        private readonly AppearanceSystem _appearance;

        internal readonly ConcurrentQueue<QueueEntry> _queuedTextures;

        private readonly TimeSpan _deferInterval = TimeSpan.FromMilliseconds(200);

        private ISawmill _sawmill;

        public ContentSpriteControl(AppearanceSystem appearance)
        {
            IoCManager.InjectDependencies(this);
            _sawmill = _logMan.GetSawmill("sprite.export");

            _appearance = appearance;
            _queuedTextures = new();
        }

        /// <summary>
        /// Enqueues rendering request.
        /// </summary>
        public void Enqueue(QueueEntry entry)
        {
            entry.NextAttempt = _timing.RealTime;
            _queuedTextures.Enqueue(entry);
        }

        /// <summary>
        /// Cancels all pending export tasks.
        /// </summary>
        public void CancelAllQueued()
        {
            var cancelled = 0;
            while (_queuedTextures.TryDequeue(out var queued))
            {
                try
                {
                    queued.Tcs.TrySetCanceled();
                }
                catch { }

                try
                {
                    queued.Texture.Dispose();
                }
                catch { }

                cancelled++;
            }
        }

        protected override void Draw(DrawingHandleScreen handle)
        {
            base.Draw(handle);

            var deferred = new List<QueueEntry>();

            while (_queuedTextures.TryDequeue(out var queued))
            {
                if (queued.Tcs.Task.IsCanceled)
                {
                    try
                    {
                        queued.Texture.Dispose();
                    }
                    catch { }

                    continue;
                }

                var now = _timing.RealTime;
                if (queued.NextAttempt > now)
                {
                    deferred.Add(queued);
                    continue;
                }

                if (ShouldBeDeffered(queued))
                {
                    queued.NextAttempt = now + _deferInterval;
                    deferred.Add(queued);
                    continue;
                }

                HandleQueue(queued, handle);
            }

            foreach (var d in deferred)
                _queuedTextures.Enqueue(d);
        }

        private bool ShouldBeDeffered(QueueEntry entry)
        {
            var entity = entry.Entity;

            if (_appearance.TryGetData<TypingIndicatorState>(entity, TypingIndicatorVisuals.State, out var state))
            {
                if (state is not TypingIndicatorState.None)
                {
                    return true;
                }
            }

            return false;
        }

        private void HandleQueue(QueueEntry queued, DrawingHandleScreen handle)
        {
            try
            {
                var result = queued;

                handle.RenderInRenderTarget(queued.Texture, () =>
                {
                    handle.DrawEntity(result.Entity, result.Texture.Size / 2, result.DrawScale, Angle.Zero,
                        overrideDirection: result.Direction);
                }, Color.Transparent);

                queued.Texture.CopyPixelsToMemory<T>(image =>
                {
                    Image<T> clone;
                    try
                    {
                        clone = image.Clone();
                    }
                    catch (Exception ex)
                    {
                        _sawmill.Error($"Failed to clone image for entity {queued.Entity}: {ex}");

                        queued.Tcs.TrySetException(ex);
                        return;
                    }

                    Task.Run(() =>
                    {
                        try
                        {
                            queued.Action.Invoke(queued, clone);
                            queued.Tcs.TrySetResult(true);
                        }
                        catch (Exception ex)
                        {
                            queued.Tcs.TrySetException(ex);
                        }
                        finally
                        {
                            try
                            {
                                clone.Dispose();
                            }
                            catch { }
                        }
                    });

                    try
                    {
                        queued.Texture.Dispose();
                    }
                    catch { }
                });
            }
            catch (Exception exc)
            {
                try
                {
                    queued.Texture.Dispose();
                }
                catch { }

                if (!string.IsNullOrEmpty(exc.StackTrace))
                    _sawmill.Fatal(exc.StackTrace);

                queued.Tcs.TrySetException(exc);
            }
        }

        public sealed class QueueEntry
        {
            public readonly IRenderTexture Texture;
            public readonly Direction Direction;
            public readonly EntityUid Entity;
            public readonly Vector2 DrawScale;
            public readonly TaskCompletionSource<bool> Tcs;
            public readonly Action<QueueEntry, Image<T>> Action;

            public TimeSpan NextAttempt;

            public QueueEntry(
                IRenderTexture texture,
                Direction direction,
                EntityUid entity,
                Vector2 drawScale,
                TaskCompletionSource<bool> tcs,
                Action<QueueEntry, Image<T>> action)
            {
                Texture = texture;
                Direction = direction;
                Entity = entity;
                DrawScale = drawScale;
                Tcs = tcs;
                Action = action;
                NextAttempt = TimeSpan.Zero;
            }

            public static implicit operator QueueEntry((
                IRenderTexture Texture,
                Direction Direction,
                EntityUid Entity,
                Vector2 DrawScale,
                TaskCompletionSource<bool> Tcs,
                Action<QueueEntry, Image<T>> Action) param)
            {
                return new QueueEntry(param.Texture, param.Direction, param.Entity, param.DrawScale, param.Tcs, param.Action);
            }
        }
    }
    //WL-Changes-end
}
