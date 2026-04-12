using System.Runtime.CompilerServices;

namespace Content.Shared._WL.Entity.Extensions
{
    public static class EntityQueryEnumeratorExt
    {
        /// <summary>
        /// Получает список заданных сущностей.
        /// </summary>
        /// <typeparam name="T">Компонент.</typeparam>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<Entity<T>> GetEntities<T>(this EntityQueryEnumerator<T> enumerator)
            where T : IComponent
        {
            var list = new List<Entity<T>>();
            while (enumerator.MoveNext(out var uid, out var comp))
                list.Add(new Entity<T>(uid, comp));

            return list;
        }

        /// <summary>
        /// <see cref="GetEntities{T}(EntityQueryEnumerator{T})"/>.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="enumerator"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<Entity<T1, T2>> GetEntities<T1, T2>(this EntityQueryEnumerator<T1, T2> enumerator)
            where T1 : IComponent
            where T2 : IComponent
        {
            var list = new List<Entity<T1, T2>>();
            while (enumerator.MoveNext(out var uid, out var comp1, out var comp2))
                list.Add(new Entity<T1, T2>(uid, comp1, comp2));

            return list;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<Entity<T1, T2, T3>> GetEntities<T1, T2, T3>(this EntityQueryEnumerator<T1, T2, T3> enumerator)
            where T1 : IComponent
            where T2 : IComponent
            where T3 : IComponent
        {
            var list = new List<Entity<T1, T2, T3>>();
            while (enumerator.MoveNext(out var uid, out var comp1, out var comp2, out var comp3))
                list.Add(new Entity<T1, T2, T3>(uid, comp1, comp2, comp3));

            return list;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<Entity<T1, T2, T3, T4>> GetEntities<T1, T2, T3, T4>(this EntityQueryEnumerator<T1, T2, T3, T4> enumerator)
            where T1 : IComponent
            where T2 : IComponent
            where T3 : IComponent
            where T4 : IComponent
        {
            var list = new List<Entity<T1, T2, T3, T4>>();
            while (enumerator.MoveNext(out var uid, out var comp1, out var comp2, out var comp3, out var comp4))
                list.Add(new Entity<T1, T2, T3, T4>(uid, comp1, comp2, comp3, comp4));

            return list;
        }
    }
}
