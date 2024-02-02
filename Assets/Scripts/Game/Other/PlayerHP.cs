using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Game.Other
{
    public class PlayerHP : IHPScript
    {
        [SerializeField] public bool CanRaiseOnRespawning;
        [SerializeField] public bool CanBeRespawned;



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


        //Івент про респавн
        public delegate void RespawningEventHandler(object sender, RespawningEventArgs e);
        public event RespawningEventHandler OnRespawning;




        /// <summary>
        /// Публічний метод для івенту про респавн гравця
        /// </summary>
        /// <param name="sender">Посилання на того хто викликав функцію</param>
        /// <param name="e">Параметру івенту</param>
        /// <remarks></remarks>
        public void Respawn(object sender, DyingEventArgs e)
        {
            //перевіряємо чи можна викликати івент OnRespawning
            if (!CanRaiseOnRespawning) return;


            RespawningEventArgs args = new RespawningEventArgs(this.gameObject);
            OnRespawning?.Invoke(this, args);
        }




        /// <summary>
        /// Публічний метод для самого респавну гравця
        /// </summary>
        /// <remarks></remarks>
        private void RespawnPlayer(object sender, RespawningEventArgs e)
        { 
            
        }



        private void Start()
        {
            if (!IsOwner) return;
            //Робимо початкову підготовку змінних
            StartDataAssign();


            //обнуляє дії, які потрібно виконати під час виклику івентів
            OnRespawning = null;


            //////////////////////////////////Записуємо дії до івентів
            //
            //
            //
            //
            //Записуємо дії до івенту про респавн гравця
            OnRespawning += RespawnPlayer;

            //Записуємо дії до івенту про смерть гравця
            OnDying += Respawn;
        }
    }
}
