using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoGame
{
    /// <summary>
    /// Шаблон, по которому создаются объекты.
    /// Описывает действия при инициации объекта и 
    /// действия при обновлении объекта.
    /// </summary>
    public interface IPattern
    {
        /// <summary>
        /// аниматор, присваиваемый объекту при создании
        /// </summary>
        public string AnimatorName { get; }
        /// <summary>
        /// анимация, присваиваемая объекту при создании
        /// </summary>
        public string InitialAnimation { get; }
        /// <summary>
        /// хитбокс, присваиваемый объекту при создании
        /// </summary>
        public Rectangle Hitbox { get; }
        /// <summary>
        /// если true, объкт при создании не будет иметь аниматора
        /// </summary>
        public bool IsHitBoxOnly { get; }
        /// <summary>
        /// объекты, реализующие этот шаблон
        /// </summary>
        public List<GameObject> Editions { get; set; }

        /// <summary>
        /// Обновление объекта.
        /// Выполняется после обновления поведений объекта.
        /// </summary>
        /// <param name="member"></param>
        /// <param name="state"></param>
        public void UpdateMember(GameObject member, IGameState state);

        /// <summary>
        /// производит изначальную настройку объекта
        /// </summary>
        /// <param name="state"></param>
        /// <param name="member"></param>
        /// <returns></returns>
        public GameObject InitializeMember(IGameState state, GameObject member);

        /// <summary>
        /// создаёт объект по этому шаблону
        /// </summary>
        /// <param name="state"></param>
        /// <param name="position"></param>
        /// <param name="layer"></param>
        /// <param name="isMirrored"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public GameObject CreateCopy(IGameState state, Vector2 position, Layer layer, bool isMirrored)
        {
            if (IsHitBoxOnly) throw new ArgumentException("Extra arguments for HitBoxOnly Pattern");
            var edition = new GameObject(state, AnimatorName, InitialAnimation, Hitbox, position, layer, isMirrored);
            ConnectMember(state, edition);
            return edition;
        }

        /// <summary>
        /// создаёт объект без аниматора по этому шаблону
        /// </summary>
        /// <param name="state"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        public GameObject CreateCopy(IGameState state, Vector2 position)
        {
            var edition = new GameObject(state, Hitbox, position);
            ConnectMember(state, edition);
            return edition;
        }

        private void ConnectMember(IGameState state, GameObject member)
        {
            member = InitializeMember(state, member);
            member.Pattern = this;
            Editions.Add(member);
        }

        /// <summary>
        /// Обновляет объекты, в соответсвем с предписаниями шаблона.
        /// Обновление происходит после обновления поведений объекта
        /// </summary>
        /// <param name="state"></param>
        public void Update(IGameState state)
        {
            foreach (GameObject member in Editions)
                UpdateMember(member, state);
        }

        public void LinkFamilies(Family[] families)
        {
            foreach (var family in families)
                family.Patterns.Add(this);
        }
    }
}