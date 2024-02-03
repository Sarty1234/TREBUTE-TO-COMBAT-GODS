using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;
using Player;



namespace Player
{
    public abstract class IHPScript : NetworkBehaviour
    {
        //////////////////////////////////////////Оголошуємо змінні
        //
        //
        //
        //
        //змінні пов'язані з хп
        [SerializeField] public bool CanRaiseOnTakingDamageEvent;
        [SerializeField] public bool CanBeDamaged;

        [SerializeField] public bool CanRaiseOnHealingEvent;
        [SerializeField] public bool CanBeHealed;

        [SerializeField] public bool CanRaiseOnAddingMaxHP;
        [SerializeField] public bool CanAddMaxHP;


        public float ActualMaxHP;
        public float CurentMaxHP;
        public float CurentHP;
        public maxHPAdditionClass[] maxHPAdditions;


        //змінні пов'язані з множником вхідної шкоди
        [SerializeField] public bool CanRaiseOnRecivingDamageMultiplayerEvent;
        [SerializeField] public bool CanAddDamageMultiplier;


        public damageMuliplayerClass[] damageMultipliers;



        //////////////////////////////////////////оголошуємо класи необхідні для роботи модуля
        //
        //
        //
        //
        //Клас множника вхідної шкоди
        public class damageMuliplayerClass
        {
            public float multiplayer;
            public float time;
        }


        //Клас збільшення максимального хп
        public class maxHPAdditionClass
        {
            public float hpAddition;
            public float time;
        }

        PlayerEventsHandler _PlayerEventsHandler;

        /// <summary>
        /// Робить початкову ініціалізацію данних
        /// </summary>
        /// <remarks></remarks>
        public virtual void StartDataAssign()
        {
            if (!IsOwner) return;
            //Присвоює дані про хп
            CurentHP = CurentMaxHP = ActualMaxHP;
            damageMultipliers = new damageMuliplayerClass[0];
            maxHPAdditions = new maxHPAdditionClass[0];


            //обнуляє дії, які потрібно виконати під час виклику івентів
            /*OnTakingDamage = null;
            OnRecivingDamageMultiplayer = null;
            OnHealing = null;
            OnAddingMaxHP = null;
            OnDying = null;
            OnUpdate = null;*/
            _PlayerEventsHandler = new PlayerEventsHandler();


            //////////////////////////////////Записуємо дії до івентів
            //
            //
            //
            //
            //Записуємо дії до івенту про отримання шкоди
            _PlayerEventsHandler.OnTakingDamage += CalculateDamage;

            //Записуємо дії до івенту про хілення
            PlayerEventsHandler.Instance.OnHealing += HealObject;

            //Записуємо дії до івенту про збільшення максимального хп
            PlayerEventsHandler.Instance.OnAddingMaxHP += AddMaxHPAdditionToList;

            //Записуємо дії до івенту про отримання бустеру вхідної шкоди
            PlayerEventsHandler.Instance.OnRecivingDamageMultiplayer += AddDamageMultiplyerToList;

            //Записуємо дії до івенту про виклик функції Update
            PlayerEventsHandler.Instance.OnUpdate += CalculateMaxHP;
        }




        /// <summary>
        /// Публічний метод для нанесення шкоди
        /// </summary>
        /// <param name="damage">Шкода, яку потрібно нанести</param>
        /// <param name="damageSource">Посилання на джерело шкоди</param>
        /// <param name="linkToPlayerWhoDamaged">Посилання на гравця, від якого шкода була нанесена</param>
        /// <remarks></remarks>
        public void TakeDamage(float damage, GameObject damageSource, GameObject linkToPlayerWhoDamaged)
        {
            if (!IsOwner) return;
            //перевіряємо чи можна викликати івент OnTakingDamage
            if (!CanRaiseOnTakingDamageEvent) return;

            Debug.Log("Damage taken");

            PlayerEventsHandler.TakingDamageEventArgs args = new PlayerEventsHandler.TakingDamageEventArgs(damage, damageSource, linkToPlayerWhoDamaged);
            PlayerEventsHandler.InvokeOnTakingDamage(this, args);
        }




        /// <summary>
        /// Приватний метод який обраховує нанесену шкоду і наносить її об'єкту
        /// </summary>
        /// <param name="sender">Посилання на того хто викликав функцію</param>
        /// <param name="e">Параметру івенту</param>
        /// <remarks></remarks>
        private void CalculateDamage(object sender, PlayerEventsHandler.TakingDamageEventArgs e)
        {
            if (!IsOwner) return;
            //перевіряємо чи можна наносити шкоду
            if (!CanBeDamaged) return;


            //вилучаємо множники шкоди чий час пройшов
            damageMultipliers = damageMultipliers.Where(obj => obj.time >= Time.time).ToArray();


            //обраховуємо нанесену шкоду
            damageMuliplayerClass defoultDamageMuliplayer = new damageMuliplayerClass()
            {
                multiplayer = 1,
                time = -1
            };
            float damageMultiplier = damageMultipliers.DefaultIfEmpty(defoultDamageMuliplayer).First().multiplayer;
            float damage = e.damage * damageMultiplier;


            //наносимо шкоду
            CurentHP -= damage;


            //дивимося чи живий обєкт
            if (CurentHP <= 0)
            {
                //якщо помер то викликаємо івент про його смерть
                PlayerEventsHandler.DyingEventArgs args = new PlayerEventsHandler.DyingEventArgs(e.linkToPlayerWhoDamaged);
                PlayerEventsHandler.InvokeOnDying(this, args);
            }
        }




        /// <summary>
        /// Публічний метод для отримання бустерів вхідної шкоди
        /// </summary>
        /// <param name="damgeMultiplyer">Бустер вхідної шкоди</param>
        /// <param name="duration">Час дії бустера вхідної шкоди</param>
        /// <param name="linkToBooster">Посилання на об'єкт який накладає бустер</param>
        /// <remarks></remarks>
        public void RecieveDamageMultiplyer(float damgeMultiplyer, float duration, GameObject linkToBooster)
        {
            if (!IsOwner) return;
            //перевіряємо чи можна викликати івент OnRecivingDamageMultiplayer
            if (!CanRaiseOnRecivingDamageMultiplayerEvent) return;


            PlayerEventsHandler.RecivingDamageMultiplayerEventArgs args = new PlayerEventsHandler.RecivingDamageMultiplayerEventArgs(damgeMultiplyer, duration, linkToBooster);
            PlayerEventsHandler.InvokeOnRecivingDamageMultiplayer(this, args);
        }




        /// <summary>
        /// Приватний метод який добавляє обє'кту бустер вхідної шкоди
        /// </summary>
        /// <param name="sender">Посилання на того хто викликав функцію</param>
        /// <param name="e">Параметру івенту</param>
        /// <remarks></remarks>
        private void AddDamageMultiplyerToList(object sender, PlayerEventsHandler.RecivingDamageMultiplayerEventArgs e)
        {
            if (!IsOwner) return;
            //перевіряємо чи можна додавати бустери вхідної шкоди
            if (!CanAddDamageMultiplier) return;


            //створюємо клас бустеру вхідної шкоди
            damageMuliplayerClass damageMuliplayer = new damageMuliplayerClass()
            {
                multiplayer = e.multiplayer,
                time = e.time + Time.time
            };


            //добавляємо клас бустеру вхідної шкоди до реєстру нашого об'єкту
            damageMultipliers = damageMultipliers.Append(damageMuliplayer).ToArray();
        }




        /// <summary>
        /// Публічний метод для отримання бустерів максимального хп
        /// </summary>
        /// <param name="HPAddition">Бустер максимального хп</param>
        /// <param name="duration">Час дії бустера вхідної шкоди</param>
        /// <param name="linkToBooster">Посилання на об'єкт який накладає бустер</param>
        /// <remarks></remarks>
        public void AddMaxHPMultiplyer(float HPAddition, float duration, GameObject linkToBooster)
        {
            if (!IsOwner) return;
            //перевіряємо чи можна викликати івент OnRecivingDamageMultiplayer
            if (!CanRaiseOnRecivingDamageMultiplayerEvent) return;


            PlayerEventsHandler.AddingMaxHPEventArgs args = new PlayerEventsHandler.AddingMaxHPEventArgs(HPAddition, duration, linkToBooster);
            PlayerEventsHandler.InvokeOnAddingMaxHP(this, args);
        }




        /// <summary>
        /// Приватний метод який добавляє обє'кту максимальне хп
        /// </summary>
        /// <param name="sender">Посилання на того хто викликав функцію</param>
        /// <param name="e">Параметру івенту</param>
        /// <remarks></remarks>
        private void AddMaxHPAdditionToList(object sender, PlayerEventsHandler.AddingMaxHPEventArgs e)
        {
            if (!IsOwner) return;
            //перевіряємо чи можна максимальне хп
            if (!CanRaiseOnAddingMaxHP) return;


            //створюємо клас бустеру максимального хп
            maxHPAdditionClass maxHPAddition = new maxHPAdditionClass()
            {
                hpAddition = e.maxHPChange,
                time = e.time + Time.time
            };


            //добавляємо клас бустеру вхідної шкоди до реєстру нашого об'єкту
            maxHPAdditions = maxHPAdditions.Append(maxHPAddition).ToArray();


            //хілимо об'єкт на кількість доданого хп
            Heal(e.maxHPChange, e.linkToBooster);
        }




        /// <summary>
        /// Приватний метод який обраховує максимальне хп
        /// </summary>
        /// <remarks></remarks>
        private void CalculateMaxHP()
        {
            float maxHPAddition = 0;


            //очищаємо список від застарілих елементів
            maxHPAdditions = maxHPAdditions.Where(obj => obj.time >= Time.time).ToArray();


            //розраховуємо нове максимальне хп
            foreach (maxHPAdditionClass item in maxHPAdditions)
            {
                maxHPAddition += item.hpAddition;
            }


            //присвоюємо нове максимальне хп
            CurentMaxHP = ActualMaxHP + maxHPAddition;
        }




        /// <summary>
        /// Публічний метод для отримання хілу
        /// </summary>
        /// <param name="heal">На скільки хілити</param>
        /// <param name="linkToHealer">Посилання на об'єкт який хілить</param>
        /// <remarks></remarks>
        public void Heal(float heal, GameObject linkToHealer)
        {
            if (!IsOwner) return;
            //перевіряємо чи можна викликати івент OnRecivingDamageMultiplayer
            if (!CanRaiseOnHealingEvent) return;


            PlayerEventsHandler.HealingEventArgs args = new PlayerEventsHandler.HealingEventArgs(heal, linkToHealer);
            PlayerEventsHandler.InvokeOnHealing(this, args);
        }




        /// <summary>
        /// Приватний метод для отримання хілу
        /// </summary>
        /// <param name="sender">Посилання на того хто викликав функцію</param>
        /// <param name="e">Параметру івенту</param>
        /// <remarks></remarks>
        private void HealObject(object sender, PlayerEventsHandler.HealingEventArgs e)
        {
            if (!IsOwner) return;
            //перевіряємо чи можна похілити
            if (!CanBeHealed) return;


            //хілимо
            CurentHP += e.heal;


            //перевірямо чи не перевищує хп максимальне
            if (CurentHP > CurentMaxHP) CurentHP = CurentMaxHP;
        }
    }
}
