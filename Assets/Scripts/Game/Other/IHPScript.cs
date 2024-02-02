using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;




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




    //////////////////////////////////////////оголошуємо класи параметрів івентів
    //
    //
    //
    //
    //Клас для параметрів івенту OnTakingDamage
    public class TakingDamageEventArgs {
        /// <summary>
        /// Робить початкову ініціалізацію данних при створенні класу
        /// </summary>
        /// <param name="_damage">Отримана шкода</param>
        /// <param name="_damageSource">Джерело отриманої шкоди</param>
        /// <param name="_linkToPlayerWhoDamaged">Посилання на гравця який наніс шкоду</param>
        /// <remarks></remarks>
        public TakingDamageEventArgs(float _damage, GameObject _damageSource, GameObject _linkToPlayerWhoDamaged) { 
            damage = _damage;
            damageSource = _damageSource;
            linkToPlayerWhoDamaged = _linkToPlayerWhoDamaged;
        }



        public float damage { get; }
        public GameObject damageSource { get; }
        public GameObject linkToPlayerWhoDamaged { get; }
    }


    //Клас для параметрів івенту OnRecivingDamageMultiplayer
    public class RecivingDamageMultiplayerEventArgs
    {
        /// <summary>
        /// Робить початкову ініціалізацію данних при створенні класу
        /// </summary>
        /// <param name="_multiplayer">Отриманий множник вхідної шкоди</param>
        /// <param name="_time">Джерело отриманої шкоди</param>
        /// <param name="_linkToBooster">Посилання на гравця який наніс шкоду</param>
        /// <remarks></remarks>
        public RecivingDamageMultiplayerEventArgs(float _multiplayer, float _time, GameObject _linkToBooster)
        {
            multiplayer = _multiplayer;
            time = _time;
            linkToBooster = _linkToBooster;
        }


        public float multiplayer { get; }
        public float time { get; }
        public GameObject linkToBooster { get; }
    }



    //Клас для параметрів івенту OnHealing
    public class HealingEventArgs
    {
        /// <summary>
        /// Робить початкову ініціалізацію данних при створенні класу
        /// </summary>
        /// <param name="_heal">Отриманий хіл</param>
        /// <param name="_linkToHealer">Посилання на гравця який похілив</param>
        /// <remarks></remarks>
        public HealingEventArgs(float _heal, GameObject _linkToHealer)
        {
            heal = _heal;
            linkToHealer = _linkToHealer;
        }


        public float heal { get; }
        public GameObject linkToHealer { get; }
    }



    //Клас для параметрів івенту OnAddingMaxHP
    public class AddingMaxHPEventArgs
    {
        /// <summary>
        /// Робить початкову ініціалізацію данних при створенні класу
        /// </summary>
        /// <param name="_maxHPChange">Отриманий додаток до максимального хп</param>
        /// <param name="_time">Джерело отриманої шкоди</param>
        /// <param name="_linkToBooster">Посилання на гравця який наніс шкоду</param>
        /// <remarks></remarks>
        public AddingMaxHPEventArgs(float _maxHPChange, float _time, GameObject _linkToBooster)
        {
            maxHPChange = _maxHPChange;
            time = _time;
            linkToBooster = _linkToBooster;
        }


        public float maxHPChange { get; }
        public float time { get; }
        public GameObject linkToBooster { get; }
    }




    //Клас для параметрів івенту OnDying
    public class DyingEventArgs
    {
        /// <summary>
        /// Робить початкову ініціалізацію данних при створенні класу
        /// </summary>
        /// <param name="_linkToLastPlayerWhoDamaged">Посилання на гравця який останнім наніс шкоду</param>
        /// <remarks></remarks>
        public DyingEventArgs(GameObject _linkToLastPlayerWhoDamaged)
        {
            lastPlayerWhoDamaged = _linkToLastPlayerWhoDamaged;
        }


        public GameObject lastPlayerWhoDamaged { get; }
    }



    ////////////////////////////////////////Оголошуємо івенти
    //
    //
    //
    //
    //Івент про отримання шкоди
    public delegate void TakingDamageEventHandler(object sender, TakingDamageEventArgs e);
    public event TakingDamageEventHandler OnTakingDamage;

    //Івент про отримання бустеру вхідної шкоди
    public delegate void RecivingDamageMultiplayerEventHandler(object sender, RecivingDamageMultiplayerEventArgs e);
    public event RecivingDamageMultiplayerEventHandler OnRecivingDamageMultiplayer;

    //Івент про отримання хілу
    public delegate void HealingEventHandler(object sender, HealingEventArgs e);
    public event HealingEventHandler OnHealing;

    //Івент про збільшення максимального хп
    public delegate void AddingMaxHPEventHandler(object sender, AddingMaxHPEventArgs e);
    public event AddingMaxHPEventHandler OnAddingMaxHP;

    //Івент про смерть
    public delegate void DyingEventHandler(object sender, DyingEventArgs e);
    public event DyingEventHandler OnDying;


    //Івент про виклик Update
    public delegate void UpdateHandler();
    public event UpdateHandler OnUpdate;

    

    /// <summary>
    /// Робить початкову ініціалізацію данних
    /// </summary>
    /// <remarks></remarks>
    public virtual void StartDataAssign()
    {
        //Присвоює дані про хп
        CurentHP = CurentMaxHP = ActualMaxHP;
        damageMultipliers = new damageMuliplayerClass[0];
        maxHPAdditions = new maxHPAdditionClass[0];


        //обнуляє дії, які потрібно виконати під час виклику івентів
        OnTakingDamage = null;
        OnRecivingDamageMultiplayer = null;
        OnHealing = null;
        OnAddingMaxHP = null;
        OnDying = null;
        OnUpdate = null;


        //////////////////////////////////Записуємо дії до івентів
        //
        //
        //
        //
        //Записуємо дії до івенту про отримання шкоди
        OnTakingDamage += CalculateDamage;

        //Записуємо дії до івенту про хілення
        OnHealing += HealObject;

        //Записуємо дії до івенту про збільшення максимального хп
        OnAddingMaxHP += AddMaxHPAdditionToList;

        //Записуємо дії до івенту про отримання бустеру вхідної шкоди
        OnRecivingDamageMultiplayer += AddDamageMultiplyerToList;

        //Записуємо дії до івенту про виклик функції Update
        OnUpdate += CalculateMaxHP;
    }


    public void Update()
    {
        if (!IsOwner) return;
        OnUpdate?.Invoke();
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


        TakingDamageEventArgs args = new TakingDamageEventArgs(damage, damageSource, linkToPlayerWhoDamaged);
        OnTakingDamage?.Invoke(this, args);
    }




    /// <summary>
    /// Приватний метод який обраховує нанесену шкоду і наносить її об'єкту
    /// </summary>
    /// <param name="sender">Посилання на того хто викликав функцію</param>
    /// <param name="e">Параметру івенту</param>
    /// <remarks></remarks>
    private void CalculateDamage(object sender, TakingDamageEventArgs e)
    {
        if (!IsOwner) return;
        //перевіряємо чи можна наносити шкоду
        if (!CanBeDamaged) return;


        //вилучаємо множники шкоди чий час пройшов
        damageMultipliers = damageMultipliers.Where(obj => obj.time >= Time.time).ToArray();


        //обраховуємо нанесену шкоду
        damageMuliplayerClass defoultDamageMuliplayer = new damageMuliplayerClass() {
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
            DyingEventArgs args = new DyingEventArgs(e.linkToPlayerWhoDamaged);
            OnDying?.Invoke(this, args);
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
        //перевіряємо чи можна викликати івент OnRecivingDamageMultiplayer
        if (!CanRaiseOnRecivingDamageMultiplayerEvent) return;


        RecivingDamageMultiplayerEventArgs args = new RecivingDamageMultiplayerEventArgs(damgeMultiplyer, duration, linkToBooster);
        OnRecivingDamageMultiplayer?.Invoke(this, args);
    }




    /// <summary>
    /// Приватний метод який добавляє обє'кту бустер вхідної шкоди
    /// </summary>
    /// <param name="sender">Посилання на того хто викликав функцію</param>
    /// <param name="e">Параметру івенту</param>
    /// <remarks></remarks>
    private void AddDamageMultiplyerToList(object sender, RecivingDamageMultiplayerEventArgs e)
    {
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
        //перевіряємо чи можна викликати івент OnRecivingDamageMultiplayer
        if (!CanRaiseOnRecivingDamageMultiplayerEvent) return;


        AddingMaxHPEventArgs args = new AddingMaxHPEventArgs(HPAddition, duration, linkToBooster);
        OnAddingMaxHP?.Invoke(this, args);
    }




    /// <summary>
    /// Приватний метод який добавляє обє'кту максимальне хп
    /// </summary>
    /// <param name="sender">Посилання на того хто викликав функцію</param>
    /// <param name="e">Параметру івенту</param>
    /// <remarks></remarks>
    private void AddMaxHPAdditionToList(object sender, AddingMaxHPEventArgs e)
    {
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
        //перевіряємо чи можна викликати івент OnRecivingDamageMultiplayer
        if (!CanRaiseOnHealingEvent) return;


        HealingEventArgs args = new HealingEventArgs(heal, linkToHealer);
        OnHealing?.Invoke(this, args);
    }




    /// <summary>
    /// Приватний метод для отримання хілу
    /// </summary>
    /// <param name="sender">Посилання на того хто викликав функцію</param>
    /// <param name="e">Параметру івенту</param>
    /// <remarks></remarks>
    private void HealObject(object sender, HealingEventArgs e)
    {
        //перевіряємо чи можна похілити
        if (!CanBeHealed) return;


        //хілимо
        CurentHP += e.heal;


        //перевірямо чи не перевищує хп максимальне
        if (CurentHP > CurentMaxHP) CurentHP = CurentMaxHP;
    }
}
