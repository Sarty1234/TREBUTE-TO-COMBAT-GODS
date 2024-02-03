using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Unity.Netcode;
using UnityEngine;


namespace Player
{
    public class PlayerEventsHandler : NetworkBehaviour
    {
        public static PlayerEventsHandler Instance;


        public PlayerEventsHandler() { 
            Instance = this;
        }



        //////////////////////////////////////////оголошуємо класи параметрів івентів
        //
        //
        //
        //
        //Клас для параметрів івенту OnTakingDamage
        public class TakingDamageEventArgs
        {
            /// <summary>
            /// Робить початкову ініціалізацію данних при створенні класу
            /// </summary>
            /// <param name="_damage">Отримана шкода</param>
            /// <param name="_damageSource">Джерело отриманої шкоди</param>
            /// <param name="_linkToPlayerWhoDamaged">Посилання на гравця який наніс шкоду</param>
            /// <remarks></remarks>
            public TakingDamageEventArgs(float _damage, GameObject _damageSource, GameObject _linkToPlayerWhoDamaged)
            {
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




        //Клас для параметрів івенту OnRespawning
        public class RespawningEventArgs
        {
            /// <summary>
            /// Робить початкову ініціалізацію данних при створенні класу
            /// </summary>
            /// <param name="_player">Посилання на гравця який респавниться</param>
            /// <remarks></remarks>
            public RespawningEventArgs(GameObject _player)
            {
                player = _player;
            }


            public GameObject player { get; }
        }



        ////////////////////////////////////////Оголошуємо івенти
        //
        //
        //
        //
        //Івент про отримання шкоди
        public delegate void TakingDamageEventHandler(object sender, TakingDamageEventArgs e);
        public event TakingDamageEventHandler OnTakingDamage;

        public static void InvokeOnTakingDamage(object sender, TakingDamageEventArgs e)
        {
            Instance.OnTakingDamage.Invoke(sender, e);
        }


        //Івент про отримання бустеру вхідної шкоди
        public delegate void RecivingDamageMultiplayerEventHandler(object sender, RecivingDamageMultiplayerEventArgs e);
        public event RecivingDamageMultiplayerEventHandler OnRecivingDamageMultiplayer;

        public static void InvokeOnRecivingDamageMultiplayer(object sender, RecivingDamageMultiplayerEventArgs e)
        {
            Instance.OnRecivingDamageMultiplayer.Invoke(sender, e);
        }


        //Івент про отримання хілу
        public delegate void HealingEventHandler(object sender, HealingEventArgs e);
        public event HealingEventHandler OnHealing;

        public static void InvokeOnHealing(object sender, HealingEventArgs e)
        {
            Instance.OnHealing.Invoke(sender, e);
        }


        //Івент про збільшення максимального хп
        public delegate void AddingMaxHPEventHandler(object sender, AddingMaxHPEventArgs e);
        public event AddingMaxHPEventHandler OnAddingMaxHP;

        public static void InvokeOnAddingMaxHP(object sender, AddingMaxHPEventArgs e)
        {
            Instance.OnAddingMaxHP.Invoke(sender, e);
        }


        //Івент про смерть
        public delegate void DyingEventHandler(object sender, DyingEventArgs e);
        public event DyingEventHandler OnDying;

        public static void InvokeOnDying(object sender, DyingEventArgs e)
        {
            Instance.OnDying.Invoke(sender, e);
        }


        //Івент про виклик Update
        public delegate void UpdateHandler();
        public event UpdateHandler OnUpdate;

        public void Update()
        {
            if (!IsOwner) return;
            OnUpdate?.Invoke();
        }


        //Івент про респавн
        public delegate void RespawningEventHandler(object sender, RespawningEventArgs e);
        public event RespawningEventHandler OnRespawning;

        public static void InvokeOnRespawning(object sender, RespawningEventArgs e)
        {
            Instance.OnRespawning.Invoke(sender, e);
        }


    }
}
